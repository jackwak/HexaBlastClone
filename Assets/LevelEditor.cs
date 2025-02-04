using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;
using UnityEditor;

public class LevelEditor : SerializedMonoBehaviour
{
    // Seçilebilir öðelerin enum'u
    public enum ItemType { None, Sword, Shield, Axe }

    // Her öðe için referans olarak GameObject atamasý
    [ShowInInspector, Title("Item Prefabs")]
    public GameObject swordPrefab;

    [ShowInInspector]
    public GameObject shieldPrefab;

    [ShowInInspector]
    public GameObject axePrefab;

    // Tablo için ItemType array (her hücrede seçilen öðeyi tutacak)
    [TableMatrix(HorizontalTitle = "Item Placement", DrawElementMethod = "DrawItemElement", ResizableColumns = false, RowHeight = 32)]
    public ItemType[,] itemGrid = new ItemType[5, 5]; // 5x5'lik tablo

    // Seçili öðe
    [ShowInInspector]
    public ItemType selectedItem = ItemType.None;

#if UNITY_EDITOR // Sadece editör modunda çalýþan kod
    // Tabloya týklayýnca seçilen öðeyi yerleþtiren method
    private ItemType DrawItemElement(Rect rect, ItemType value)
    {
        // Hücreye týklama iþlemi
        if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
        {
            value = selectedItem; // Seçilen öðeyi tabloya yerleþtir
            GUI.changed = true;
            Event.current.Use();
        }

        // Hücrenin rengini belirle
        Color cellColor = GetColorForItem(value);
        EditorGUI.DrawRect(rect.Padding(1), cellColor);

        // Hücreye öðe ismini yaz
        string displayText = value == ItemType.None ? "Empty" : value.ToString();
        EditorGUI.LabelField(rect, displayText, EditorStyles.whiteLabel);

        return value;
    }
#endif

    // Öðe tipine göre renk döndüren method
    private Color GetColorForItem(ItemType itemType)
    {
        switch (itemType)
        {
            case ItemType.Sword:
                return new Color(0.8f, 0.2f, 0.2f);  // Kýrmýzý
            case ItemType.Shield:
                return new Color(0.2f, 0.8f, 0.2f); // Yeþil
            case ItemType.Axe:
                return new Color(0.2f, 0.2f, 0.8f); // Mavi
            default:
                return new Color(0.5f, 0.5f, 0.5f); // Gri
        }
    }

    // Pencereyi çizme
    [OnInspectorGUI]
    private void DrawSelectionWindow()
    {
        GUILayout.Label("Select an Item", EditorStyles.boldLabel);

        // Seçim butonlarý
        if (GUILayout.Button("Sword"))
        {
            selectedItem = ItemType.Sword;
        }

        if (GUILayout.Button("Shield"))
        {
            selectedItem = ItemType.Shield;
        }

        if (GUILayout.Button("Axe"))
        {
            selectedItem = ItemType.Axe;
        }

        GUILayout.Space(10);
        GUILayout.Label("Selected Item: " + selectedItem);

        // Instantiate butonu
        if (GUILayout.Button("Instantiate Items"))
        {
            InstantiateItems();
        }
    }

    // Tablodaki öðelere göre sahnede öðe instantiate eden method
    private void InstantiateItems()
    {
        for (int x = 0; x < itemGrid.GetLength(0); x++)
        {
            for (int y = 0; y < itemGrid.GetLength(1); y++)
            {
                ItemType itemType = itemGrid[x, y];
                GameObject prefab = GetPrefabForItem(itemType);

                if (prefab != null)
                {
                    Vector3 position = new Vector3(x * 2.0f, 0, y * 2.0f); // Pozisyonu ayarla (2 birim aralýk)
                    Instantiate(prefab, position, Quaternion.identity);
                }
            }
        }
    }

    // ItemType'a göre prefab döndüren method
    private GameObject GetPrefabForItem(ItemType itemType)
    {
        switch (itemType)
        {
            case ItemType.Sword:
                return swordPrefab;
            case ItemType.Shield:
                return shieldPrefab;
            case ItemType.Axe:
                return axePrefab;
            default:
                return null;
        }
    }
}
