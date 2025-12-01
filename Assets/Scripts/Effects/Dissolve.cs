using System.Collections;
using UnityEngine.UIElements;
using UnityEngine;

public class Dissolve : MonoBehaviour
{
    [Header("Dissolve Settings")]
    [SerializeField] private float _dissolveTime = 0.75f;
    [SerializeField] private SpriteRenderer[] _spriteRenderers;
    [SerializeField] private Color _outlineColor = Color.white;
    [SerializeField] private float _intensity = 1.63f;
    private Material[] _materials;
    private int _dissolveAmount = Shader.PropertyToID("_DissolveAmount");
    private int _verticalDissolveAmount = Shader.PropertyToID("_VerticalDissolve");
    private int _outlineColorId = Shader.PropertyToID("_OutlineColor");
    private void Start()
    {
        _materials = new Material[_spriteRenderers.Length];
        for (int i = 0; i < _spriteRenderers.Length; i++)
        {
            _materials[i] = _spriteRenderers[i].material;
        }
        ApplyOutlineColor();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.V))
        {
            DissolveVanish();
        }
    }


    private void ApplyOutlineColor()
    {
        MaterialPropertyBlock mpb = new MaterialPropertyBlock();

        foreach (var sr in _spriteRenderers)
        {
            if (sr == null) continue;

            sr.GetPropertyBlock(mpb);

            Color hdrColor = _outlineColor * _intensity;

            mpb.SetColor(_outlineColorId, hdrColor);

            sr.SetPropertyBlock(mpb);
        }
    }
    public void DissolveVanish()
    {
        StartCoroutine(Vanish(true, false));
    }
    private IEnumerator Vanish(bool useDissolve, bool useVertical)
    {
        float elapsedTime = 0f;
        while (elapsedTime < _dissolveTime)
        {
            elapsedTime += Time.deltaTime;

            float lerpedDissolve = Mathf.Lerp(0, 1.1f, (elapsedTime / _dissolveTime));
            float lerpedVerticalDissolve = Mathf.Lerp(0f, 1.1f, (elapsedTime / _dissolveTime));

            for (int i = 0; i < _materials.Length; i++)
            {
                if (useDissolve) _materials[i].SetFloat(_dissolveAmount, lerpedDissolve);
                if (useVertical) _materials[i].SetFloat(_verticalDissolveAmount, lerpedVerticalDissolve);
            }
            yield return null;
        }
    }

    private IEnumerator Apper(bool useDissolve, bool useVertical)
    {
        float elapsedTime = 0f;
        while (elapsedTime < _dissolveTime)
        {
            elapsedTime += Time.deltaTime;

            float lerpedDissolve = Mathf.Lerp(1.1f, 0f, (elapsedTime / _dissolveTime));
            float lerpedVerticalDissolve = Mathf.Lerp(1.1f, 0f, (elapsedTime / _dissolveTime));

            for (int i = 0; i < _materials.Length; i++)
            {
                if (useDissolve) _materials[i].SetFloat(_dissolveAmount, lerpedDissolve);
                if (useVertical) _materials[i].SetFloat(_verticalDissolveAmount, lerpedVerticalDissolve);
            }
            yield return null;
        }
    }
}
