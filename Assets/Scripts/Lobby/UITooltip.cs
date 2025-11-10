using TMPro;
using UnityEngine;

public class UITooltip : MonoBehaviour
{
    public static UITooltip Instance { get; private set; }

    [SerializeField] private GameObject tooltipObject;
    [SerializeField] private TextMeshProUGUI tooltipText;
    [SerializeField] private RectTransform canvasRect;

    private RectTransform tooltipRect;

    private void Awake()
    {
        Instance = this;
        tooltipRect = tooltipObject.GetComponent<RectTransform>();
        tooltipObject.SetActive(false);
    }

    public void Show(string text)
    {
        tooltipObject.SetActive(true);
        tooltipText.text = text;
    }

    public void Hide()
    {
        tooltipObject.SetActive(false);
    }
}
