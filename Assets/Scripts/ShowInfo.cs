using UnityEngine;

public class ShowInfo : MonoBehaviour
{
    private InformationBox informationBox;
    void Start()
    {
        informationBox = GetComponentInChildren<InformationBox>();
        if (informationBox != null)
            informationBox.gameObject.SetActive(false);

    }
    public void ShowInformation()
    {
        informationBox.gameObject.SetActive(true);
    }

    public void HideInformation()
    {
        informationBox.gameObject.SetActive(false);
    }
}
