using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class ClickableText : MonoBehaviour, IPointerClickHandler
{
    TMP_Text text;

    // Start is called before the first frame update
    void Start()
    {
        text = GetComponent<TMP_Text>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //public void onpointer
    public void OnPointerClick(PointerEventData eventData)
    {
        int linkTaggedText = TMP_TextUtilities.FindIntersectingLink(text, Input.mousePosition, null);

        if(linkTaggedText != -1)
            Application.OpenURL(text.textInfo.linkInfo[linkTaggedText].GetLinkID());
    }
}
