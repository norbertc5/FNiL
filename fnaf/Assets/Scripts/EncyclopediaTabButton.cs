using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine.UI;

public class EncyclopediaTabButton : MonoBehaviour
{
    MenuManager menuManager;
    [SerializeField] Sprite imageSprite;
    [SerializeField] bool isUnlocked;

    void Start()
    {
        menuManager = FindObjectOfType<MenuManager>();
        TextMeshProUGUI textInButton = GetComponentInChildren<TextMeshProUGUI>();

        // when player hasn't discovered thing which is describing by this encyclopedia entry, button is inactive
        // don't work with console entry
        if ((PlayerPrefs.GetString(textInButton.text) != "true" && textInButton.text != "Console") ||
            (textInButton.text == "Console" && PlayerPrefs.GetString("HasFinished5thNight") != "true"))
        {
            GetComponent<Button>().enabled = false;
            GetComponentInChildren<TextMeshProUGUI>().text = "???";
            GetComponentInChildren<TextMeshProUGUI>().fontSize = MenuManager.DEFAULT_TEXT_SIZE_IN_ENCYCLOPEDIA_TAB_BUTTON;
        }
    }

    public void OnButtonClick()
    {
        // when button clicked set textInEncyclopedia to top of screen
        menuManager.textInEncyclopedia.rectTransform.position = 
            new Vector3(menuManager.textInEncyclopedia.rectTransform.position.x, menuManager.encyclopediaTextYPos);

        // set textInEncyclopedia to suitable .txt file
        string entryName = transform.GetChild(0).GetComponent<TextMeshProUGUI>().text;
        string path = Application.streamingAssetsPath + "/EncyclopediaEntries" + "/" + entryName + ".txt";
        List<string> fileContent = File.ReadAllLines(path).ToList();
        StringBuilder sb = new StringBuilder();

        foreach (string line in fileContent)
        {
            sb.AppendLine(line);
        }

        menuManager.textInEncyclopedia.text = sb.ToString();
        menuManager.imageInEncyclopedia.sprite = imageSprite;  // set image in encyclopedia
    }
}
