using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class ImageList : MonoBehaviour
{

    [SerializeField]
    private GameObject _Content;
    [SerializeField]
    private Text _SearchString;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Search ()
    {

        StartCoroutine(isearch());
    }

    IEnumerator isearch()
    {
        yield return new WaitForSeconds(.1f);
        string SearchText = _SearchString.text;
        print(_SearchString.text);
        if (SearchText == "")
        {
            foreach (var item in _Content.GetComponentsInChildren<Image>(true))
            {
                item.gameObject.SetActive(true);
            }
        }
        else
        {
            foreach (var item in _Content.GetComponentsInChildren<Image>(true))
            {
                if (item.gameObject.name.ToLower().Contains(SearchText.ToLower()))
                {
                    item.gameObject.SetActive(true);
                }
                else item.gameObject.SetActive(false);
            }
        }
    }

}
