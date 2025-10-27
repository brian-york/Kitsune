using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EnemyHPUI : MonoBehaviour
{
    public TextMeshProUGUI enemyNameText;
    public Slider hpBar;
    
    private int maxHP;
    private float targetFillAmount;
    
    public void Initialize(string enemyName, int currentHP, int maxHP)
    {
        this.maxHP = maxHP;
        enemyNameText.text = enemyName;
        hpBar.maxValue = maxHP;
        hpBar.value = currentHP;
        targetFillAmount = (float)currentHP / maxHP;
    }
    
    public void UpdateHP(int currentHP)
    {
        targetFillAmount = (float)currentHP / maxHP;
        hpBar.value = currentHP;
    }
    
    void Update()
    {
        if (hpBar.fillRect != null)
        {
            float currentFill = hpBar.fillRect.GetComponent<Image>().fillAmount;
            hpBar.fillRect.GetComponent<Image>().fillAmount = Mathf.Lerp(currentFill, targetFillAmount, Time.deltaTime * 5f);
        }
    }
}
