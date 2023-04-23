using JKFrame;
using UnityEngine;
using UnityEngine.UI;

public class UI_BuildWindow_BuildPanelItem : MonoBehaviour
{
    [SerializeField] Image m_IconImage;
    [SerializeField] Text m_CountText;
    static Color s_IsMeetColor = Color.white;
    static Color s_NotMeetColor = new Color(0.9528f, 0.4809f, 0.4809f);

    public void Show(int configID, int currCount, int needCount)
    {
        ItemConfig itemConfig = ConfigManager.Instance.GetConfig<ItemConfig>(ConfigName.Item, configID);
        m_IconImage.sprite = itemConfig.Icon;
        m_CountText.text = currCount.ToString() + "/" + needCount.ToString();
        m_CountText.color = currCount >= needCount ? s_IsMeetColor : s_NotMeetColor;
        this.Show();
    }

    public void Close()
    {
        this.Hide();
    }
}