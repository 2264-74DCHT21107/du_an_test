using System.Collections;
using UnityEngine;

public class CoinEffect : MonoBehaviour
{
    [Header("Effect Settings")]
    public float duration = 0.4f;
    public float moveUpDistance = 0.5f;
    public AnimationCurve scaleCurve;
    public AnimationCurve alphaCurve;

    private Vector3 startPos;
    private Vector3 startScale;
    private Renderer rend;
    private Material mat;
    private Color startColor;

    void Awake()
    {
        rend = GetComponent<Renderer>();
        if (rend != null)
        {
            mat = rend.material;
            startColor = mat.color;
        }

        startScale = transform.localScale;
    }

    public void PlayEffect()
    {
        startPos = transform.position;
        StopAllCoroutines();
        StartCoroutine(Animate());
    }

    IEnumerator Animate()
    {
        float time = 0f;

        while (time < duration)
        {
            float t = time / duration;


            float scaleValue = scaleCurve != null ? scaleCurve.Evaluate(t) : (1 - t);
            transform.localScale = startScale * scaleValue;


            transform.position = startPos + Vector3.up * (moveUpDistance * t);


            if (mat != null)
            {
                float alpha = alphaCurve != null ? alphaCurve.Evaluate(t) : (1 - t);
                Color c = startColor;
                c.a = alpha;
                mat.color = c;
            }

            time += Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject);
    }
}