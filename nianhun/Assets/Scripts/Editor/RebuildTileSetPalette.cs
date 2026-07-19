#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

public static class RebuildTileSetPalette
{
    private const string PalettePrefabPath = "Assets/tile set.prefab";
    private const string PrimaryTilesFolder = "Assets/Graphics/Tiles/1 Tiles";
    private const string AllTilesFolder = "Assets/Graphics/Tiles";
    private const int Columns = 16;

    [MenuItem("Tools/Tiles/Rebuild Tile Set Palette (Fix Red Tiles)")]
    public static void Rebuild()
    {
        if (!EditorUtility.DisplayDialog(
                "Rebuild Tile Set Palette",
                "将用当前磁盘上仍存在的 Tile 资源重建 Assets/tile set.prefab。\n" +
                "这会覆盖 Palette 里现有排布，但不会删除 Tile 资源本身。\n\n继续？",
                "重建",
                "取消"))
        {
            return;
        }

        List<TileBase> tiles = CollectTiles();
        if (tiles.Count == 0)
        {
            EditorUtility.DisplayDialog("Rebuild Tile Set Palette", "没有找到可用的 Tile 资源。", "OK");
            return;
        }

        GameObject paletteRoot = PrefabUtility.LoadPrefabContents(PalettePrefabPath);
        try
        {
            Grid grid = paletteRoot.GetComponent<Grid>();
            if (grid == null)
                grid = paletteRoot.AddComponent<Grid>();

            grid.cellSize = new Vector3(1f, 1f, 0f);
            grid.cellGap = Vector3.zero;
            grid.cellLayout = GridLayout.CellLayout.Rectangle;

            Tilemap tilemap = paletteRoot.GetComponentInChildren<Tilemap>(true);
            if (tilemap == null)
            {
                GameObject layer = new GameObject("Layer1", typeof(Tilemap), typeof(TilemapRenderer));
                layer.transform.SetParent(paletteRoot.transform, false);
                tilemap = layer.GetComponent<Tilemap>();
            }

            tilemap.ClearAllTiles();
            tilemap.tileAnchor = new Vector3(0.5f, 0.5f, 0f);
            tilemap.orientation = Tilemap.Orientation.XY;
            tilemap.color = Color.white;

            for (int i = 0; i < tiles.Count; i++)
            {
                int x = i % Columns;
                int y = -(i / Columns);
                tilemap.SetTile(new Vector3Int(x, y, 0), tiles[i]);
            }

            tilemap.CompressBounds();

            // Palette Settings 是内置脚本；强制 Manual，避免 Unity 按错误尺寸自动改 Cell Size
            foreach (MonoBehaviour behaviour in paletteRoot.GetComponentsInChildren<MonoBehaviour>(true))
            {
                if (behaviour == null)
                    continue;

                SerializedObject so = new SerializedObject(behaviour);
                SerializedProperty cellSizing = so.FindProperty("cellSizing");
                if (cellSizing == null)
                    continue;

                cellSizing.intValue = 1; // Manual
                so.ApplyModifiedPropertiesWithoutUndo();
            }

            PrefabUtility.SaveAsPrefabAsset(paletteRoot, PalettePrefabPath);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(paletteRoot);
        }

        AssetDatabase.ImportAsset(PalettePrefabPath, ImportAssetOptions.ForceUpdate);
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog(
            "Rebuild Tile Set Palette",
            $"已重建 Palette，共放入 {tiles.Count} 个 Tile。\n请关闭并重新打开 Tile Palette 窗口查看。",
            "OK");

        Debug.Log($"[RebuildTileSetPalette] Rebuilt '{PalettePrefabPath}' with {tiles.Count} tiles.");
    }

    private static List<TileBase> CollectTiles()
    {
        List<TileBase> result = new List<TileBase>();
        HashSet<string> seen = new HashSet<string>();

        void AddFromFolder(string folder, bool tilesetOnly)
        {
            if (!Directory.Exists(folder))
                return;

            string[] guids = AssetDatabase.FindAssets("t:TileBase", new[] { folder });
            List<TileBase> batch = new List<TileBase>();

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (string.IsNullOrEmpty(path) || !seen.Add(guid))
                    continue;

                if (tilesetOnly)
                {
                    string fileName = Path.GetFileNameWithoutExtension(path);
                    if (!fileName.StartsWith("Tileset_"))
                        continue;
                }

                TileBase tile = AssetDatabase.LoadAssetAtPath<TileBase>(path);
                if (tile != null)
                    batch.Add(tile);
            }

            batch = batch
                .OrderBy(t =>
                {
                    string name = t.name;
                    if (name.StartsWith("Tileset_") &&
                        int.TryParse(name.Substring("Tileset_".Length), out int index))
                        return index;
                    return int.MaxValue;
                })
                .ThenBy(t => t.name)
                .ToList();

            result.AddRange(batch);
        }

        // 先放主图集切片，再补其它仍存在的 tile（含子目录与根目录）
        AddFromFolder(PrimaryTilesFolder, tilesetOnly: true);
        AddFromFolder(PrimaryTilesFolder, tilesetOnly: false);
        AddFromFolder(AllTilesFolder, tilesetOnly: false);

        // 去掉 sprite 丢失的 tile，避免 Palette 再次出现洋红块
        result = result.Where(tile =>
        {
            if (tile is Tile t)
                return t.sprite != null;
            return tile != null;
        }).ToList();

        return result;
    }
}
#endif
