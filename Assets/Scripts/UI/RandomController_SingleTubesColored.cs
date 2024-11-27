

using System;
using UnityEngine;
using UnityEngine.UI;

public class RandomController_SingleTubesColored : MonoBehaviour
{
    public InputField Input_TubesColors;
    public InputField Input_TubesNums;

    public Button Button_Remove;
    public int TubesNum = 0;
    public int TubesColorsNum = 0;

    public Action OnValueChanged;
    public Action OnRemove;



    void Start()
    {
        Input_TubesColors = transform.Find("t1/Input").GetComponent<InputField>();
        Input_TubesNums = transform.Find("t2/Input").GetComponent<InputField>();
        Button_Remove = transform.Find("Del").GetComponent<Button>();
        Input_TubesColors.text = TubesColorsNum.ToString();
        Input_TubesNums.text = TubesNum.ToString();
        Input_TubesColors.onValueChanged.AddListener(delegate { TubesColorsNum = int.Parse(Input_TubesColors.text); OnValueChanged?.Invoke(); });
        Input_TubesNums.onValueChanged.AddListener(delegate { TubesNum = int.Parse(Input_TubesNums.text); OnValueChanged?.Invoke(); });
        Input_TubesColors.onValidateInput += delegate (string input, int charIndex, char addedChar)
        {
            if (char.IsDigit(addedChar) || addedChar == '\b')
            {
                return addedChar;
            }
            return '\0';
        };
        Input_TubesNums.onValidateInput += delegate (string input, int charIndex, char addedChar)
        {
            if (char.IsDigit(addedChar) || addedChar == '\b')
            {
                return addedChar;
            }
            return '\0';
        };
        Button_Remove.onClick.AddListener(delegate
        {
            OnRemove?.Invoke();
        });
    }

    public void SetTubesColorsNum(int nums)
    {
        TubesColorsNum = nums;
        if (Input_TubesColors != null)
        {
            Input_TubesColors.text = TubesColorsNum.ToString();
        }
    }

    public void SetTubesNum(int nums)
    {
        TubesNum = nums;
        if (Input_TubesNums != null)
        {
            Input_TubesNums.text = TubesNum.ToString();
        }
    }

}
