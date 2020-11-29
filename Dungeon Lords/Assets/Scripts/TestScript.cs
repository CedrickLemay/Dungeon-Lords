using Assets;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TestScript : MonoBehaviour
{
    public PlayerBoard pb;

    void Start()
    {
        
    }

    public void TestRessource(GameObject parent)
    {
        parent.active = true;

        pb = new PlayerBoard();

        GameObject cardPrefab;
        cardPrefab = Resources.Load<GameObject>("Prefab/OrderCard");
        GameObject cardIcon = cardPrefab.transform.Find("CardIcon").gameObject;

        Sprite s;
        string spritePath;

        float cardWidth = ((RectTransform)cardPrefab.transform).rect.width;
        
        const float SPACEWIDTH = 20.0f;

        int index = 1;
        foreach (PlayerBoard.OrderCard oc in pb.OrderAccessible)
        {
            spritePath = "Sprite/OrderCard/" + oc.ToString();
            s = Resources.Load<Sprite>(spritePath);
            cardIcon.GetComponent<Image>().sprite = s;

            Vector3 pos = new Vector3(0.0f, 0.0f);
            pos.x = -((((float)pb.OrderAccessible.Count / 2 - index) * (cardWidth + SPACEWIDTH)) + ((cardWidth + SPACEWIDTH) / 2));

            GameObject clone = GameObject.Instantiate(cardPrefab, parent.transform);

            clone.transform.localPosition = pos;
            index++;
        }
        
    }
}
