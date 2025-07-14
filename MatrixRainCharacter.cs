using UnityEngine;
using TMPro;

public class MatrixRainCharacter : MonoBehaviour
{
    public TMP_Text tmpText;
    public float speed = 20f;
    private string charset = "アイウエオカキクケコサシスセソ0123456789";

    void Awake()
    {
        if (tmpText == null)
            tmpText = GetComponent<TMP_Text>();
    }

    public void SetRandomCharacter()
    {
        if (tmpText == null) return;
        int index = Random.Range(0, charset.Length);
        tmpText.text = charset[index].ToString();
    }
}