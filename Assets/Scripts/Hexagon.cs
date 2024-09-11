using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Hexagon : MonoBehaviour
{
    [Header("Elements")]
    [SerializeField] private Renderer _renderer;
    [SerializeField] private Collider _collider;
    public HexagonStack HexagonStack { get; private set; }

    public Color Color
    {
        get => _renderer.material.color;
        set => _renderer.material.color = value;
    }

    public void Configure(HexagonStack hexagonStack)
    {
        HexagonStack = hexagonStack;
    }

    public void SetParent(Transform parent)
    {
        transform.SetParent(parent);
    }

    public void DisableCollider() => _collider.enabled = false;

    public void MoveToLocal(Vector3 targetLocalPosition)
    {
        LeanTween.cancel(gameObject);

        float delay = transform.GetSiblingIndex() * .01f;

        /*int totalSiblings = transform.parent.childCount;
        float delay = (totalSiblings - transform.GetSiblingIndex() - 1) * .01f;*/

        LeanTween.moveLocal(gameObject, targetLocalPosition, .2f)
            .setEase(LeanTweenType.easeInOutSine)
            .setDelay(delay);

        Vector3 direction = (targetLocalPosition - transform.position).With(y: 0).normalized;
        Vector3 rotationAxis = Vector3.Cross(Vector3.up, direction);

        LeanTween.rotateAround(gameObject, rotationAxis, 180, .2f)
            .setEase(LeanTweenType.easeInOutSine)
            .setDelay(delay);
    }

    public void Vanish(float delay)
    {
        LeanTween.cancel(gameObject);

        LeanTween.scale(gameObject, Vector3.zero, .2f)
            .setEase(LeanTweenType.easeInBack)
            .setDelay(delay).setOnComplete(()=> Destroy(gameObject));
    }
}
