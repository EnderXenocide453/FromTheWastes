using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class AlertPanel : MonoBehaviour
{
    [SerializeField] private TMP_Text alertText;
    [SerializeField] private float fadeSpeed = 0.1f;
    [SerializeField] private float fadeTime = 5;

    private Coroutine _fading;

    private void Start()
    {
        if (!alertText)
            alertText = GetComponent<TMP_Text>();
    }

    public void Alert(string text, Color color)
    {
        if (_fading != null)
            StopCoroutine(_fading);

        alertText.text = text;
        alertText.color = color;

        _fading = StartCoroutine(Fade());
    }

    private IEnumerator Fade()
    {
        float alpha = 1;
        while (alpha > 0) {
            alpha -= 1 / fadeTime / fadeSpeed;
            alertText.alpha = alpha;
            
            yield return new WaitForSeconds(1 / fadeSpeed);
        }
    }
}
