using UnityEngine;

public class UIBase : MonoBehaviour
{
    public UIType Type { get; private set; }
    public string Address { get; private set; }

    public void SetOpenInfo(UIType type, string address)
    {
        Type = type;
        Address = address;
    }

    public void SetVisible(bool visible)
    {
        gameObject.SetActive(visible);
    }
}
