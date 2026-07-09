# 时之王二阶段：从「绑了事件却没伤害」到完整 Phase 2

> 日期：2026-07-09  
> 主题：TimeKing Boss 战斗系统迭代

今天把时之王从「单阶段近战 Boss」推进到了带变身、新招式、追击和技能命中修复的完整二阶段。过程里踩了不少「看起来绑对了、实际静默失败」的坑，记录一下。

---

## 一、攻击有时不生效：事件不是唯一真相

一开始的问题很典型：动画事件已经绑了，但伤害时有时无。

排查后发现，伤害链路其实有多层闸门：

1. 当前状态必须还是 `TimeKingAttackState` / `JumpAttackState`
2. 招式类型要和事件函数匹配（多段用 `attackXhitY`，单段用 `attack2hit1` / `attacktrigger`）
3. 判定框里要真的碰到玩家

其中最坑的是：**攻击时长 `statetimer` 会比动画事件更早结束**。状态已经切到 Recovery / 下一招时，事件再触发会被直接 `return`，看起来就像「绑了没用」。

另外还有一个隐蔽逻辑：`TryDealSegmentDamage` 先把 hit key 写进集合再做 overlap。空挥一次后，同段再触发也会被挡掉。

### 做法

把出伤改成**时间驱动为主、动画事件为辅**：

- 多段招：按各段 `hitTime` 结算
- Attack2 / Jump：用 `attack2HitTime` / `jumpAttackHitTime`
- 两边共用去重，不会打两次

事件可以留着当备份，但不再是唯一依赖。

---

## 二、二阶段：2/3 血变身，解锁 5 / 6 / 7

对齐 RootBoss 的变身流程：

- 阈值：`currentHealth <= maxHealth * 2/3`
- 进入 `TimeKingChangeState`：清连招、关反击窗、锁血无敌、播 `change`
- 结束后 `IsPhase2 = true`，解锁 Attack5 / 6 / 7

招式结构：

| 招式 | 段数 | 用途 |
|------|------|------|
| Attack5 | 3 段 | 二阶段主力 |
| Attack6 | 2 段 | 只出现在连招里 |
| Attack7 | 2 段 | 连招收尾 / 单放 |

二阶段连招池替换一阶段旧池：

- Combo：`1-7`、`2-6`、`3-2-6`、`5-2`、`7-6`、`4-7`
- Solo：`1`、`2`、`3`、`5`、`7`（4、6 不单放）

一阶段 Attack1–4 的 CD 在二阶段不是改成 6 秒，而是**在原 CD 上再加 6 秒**，让新招更容易被选中。

---

## 三、远处追击：3 → 10 按距离平滑变速

二阶段下，当前招式结束时如果玩家距离超过 `postAttackChaseDistance`：

- 中断剩余连招
- 进入 `TimeKingChaseState`
- 不再走 Recovery 站桩

速度规则：

- 近（≈近战距离）→ **3**
- 远（到 `chaseSpeedFarDistance`）→ **10**
- 中间用 smoothstep 插值，再用平滑系数过渡，避免速度跳变

贴脸变慢、拉开加速，手感比固定追击速度自然很多。

---

## 四、Attack5/6/7「没伤害」：Prefab 没配判定点

代码里默认 segments 写好了，但 prefab 序列化后 Attack5/6/7 的 `hitCheck` 为空。  
`DealDamageAtHitArea` 遇到 `hitCheck == null` 会直接 return——事件和时间出伤都会变成空挥。

修复：

1. 运行时空 `hitCheck` 回退到 `attackcheck`
2. prefab 补上 5/6/7 的 segments 与 hitTime
3. Awake 时统一 `RepairHitChecks`

这类问题很适合用 Gizmo 一眼看出来：圈都没有，就别指望有伤害。

---

## 五、为什么几乎看不到带 Attack6 的连招

Attack6 没有单放，只活在 `2-6`、`3-2-6`、`7-6` 里。  
旧的 `IsComboExecutable` 要求连招里**每一招 CD 都就绪**。

二阶段 Attack2 变成 `4 + 6 = 10s` 后，`2-6` / `3-2-6` 经常整段被否掉；再叠加连招只有 40% 尝试率，Attack6 几乎绝迹。

调整：

- 连招只检查**第一招**是否就绪
- 二阶段连招权重提到约 70%
- 单放都不可用时，仍从可用连招里兜底

这样 `7-6` 会稳定出现，`2-6` 也会在 Attack2 就绪时正常进池。

---

## 六、两个「看起来无关」的红错与漏伤

### 1. 暴击震屏空引用

TimeKing 打出暴击时走 `EntityFx.ScreenShake()`，但 Boss 身上没有 `CinemachineImpulseSource`。  
加了空检查：没有组件就跳过，不再炸 Console。

### 2. Laser / Strike 打后摇 Boss 偶尔没伤害

技能只靠 `OnTriggerEnter2D`。Boss 已经在判定里、或后摇时碰撞体刚启用时，不会再触发 Enter。

改成：

- `GetComponentInParent<Enemy>()` 找目标
- Laser / Spin：启用时和每跳伤害前主动 `OverlapCollider`
- Strike：启用后连续扫几帧，避免判定框晚开漏打

---

## 七、今天留下的状态机图

```text
Enter → Idle → Battle ⇄ Attack / Jump
                 ↓ HP ≤ 2/3
              Change（无敌）
                 ↓
         Battle（Phase2 新连招）
                 ↓ 攻击结束且玩家很远
              Chase（速度 3↔10）
                 ↓ 回到近战
              Battle
```

---

## 八、还需要在 Unity 里确认的事

代码侧已经齐了，场景侧仍建议核对：

1. Animator 是否有 `change`、`attack5`、`attack6`、`attack7`
2. Prefab 上 5/6/7 的判定点、hitTime、Duration 是否和动画对齐
3. 场景里旧 TimeKing 实例是否已 Apply Prefab（追击速度、新字段）
4. 黄字警告：`TimeKingRecovery` / `TimeKingBattle` 参数不存在——状态机用 `anim.Play` 播地面动画，这些 bool 参数可以后续从 Animator 清理或补上

---

## 小结

今天最有价值的不是「又加了三个招式」，而是把几条容易静默失败的链路补牢了：

- 出伤不要只信动画事件
- Prefab 序列化会吞掉默认判定点
- 连招 CD 检查过严会让后半段招式永远选不中
- Trigger Enter 不是可靠的技能命中方式

Boss 战手感，往往死在这些「偶尔」上。
