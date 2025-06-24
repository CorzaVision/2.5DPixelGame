using UnityEngine;

[CreateAssetMenu(fileName = "CurrencyData", menuName = "Economy/Currency")]
public class CurrencyData : ScriptableObject
{
    [Header("Basic  Information")]
    // ID, Name, Description, Icon
    public int currencyID;
    public string currencyName;
    public string currencyDescription;
    public Texture icon;

    [Header("Currency Properties")]
    // Max Amount for the currency type
    // Conversion rate ( how many of this currency is needed to get 1 of the other currency)
    // is this the highest currency in the game? (Gold)
    public int maxAmount = 99; // Copper is the lowest currency, so it has the lowest max amount
    public int conversionRate = 100; // 100 copper = 1 silver
    public bool isHighestCurrency = false; // Gold is the highest currency, so it is the highest currency

    [Header("Display Settings")]
    //Display format, color, etc.
    public string displayFormat = "0.00"; // 100.00
    public Color displayColor = Color.yellow; // Yellow

    [Header("Drop Settings")]
    //Drop rate, drop min/maxamounts, level scaling

    public int minDropAmount = 1;
    public int maxDropAmount = 10;
    public int dropChance = 50;
    public bool levelScaling = true;
    public float levelScalingMultiplier = 1.0f;

}
  