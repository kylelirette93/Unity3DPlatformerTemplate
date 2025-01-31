using UnityEngine;
using System;
using DG.Tweening;

/// <summary>
/// Trigger action that creates and executes DOTween animations.
/// Supports movement, rotation, scaling, and fade animations with various modes and settings.
/// </summary>
[Serializable]
public class DOTweenAction : TriggerAction
{
    /// <summary>
    /// Defines the type of property to animate
    /// </summary>
    public enum TweenType
    {
        Move,       // Animate position
        Rotate,     // Animate rotation
        Scale,      // Animate scale
        Fade        // Animate transparency (requires CanvasGroup for UI elements)
    }
    
    /// <summary>
    /// Defines how the animation should behave
    /// </summary>
    public enum TweenMode
    {
        To,         // Animate to a target value
        From,       // Animate from a starting value
        Punch,      // Create a punch effect that returns to original
        Shake       // Create a shake effect that returns to original
    }
    
    [Tooltip("The object to animate")]
    [SerializeField] private Transform target;

    [Tooltip("What property of the object to animate")]
    [SerializeField] private TweenType tweenType = TweenType.Move;

    [Tooltip("How the animation should behave")]
    [SerializeField] private TweenMode tweenMode = TweenMode.To;

    [Tooltip("Target value for the animation (interpretation depends on TweenType)")]
    [SerializeField] private Vector3 value;

    [Tooltip("How long the animation should take")]
    [SerializeField] private float duration = 1f;

    [Tooltip("The easing function to use")]
    [SerializeField] private Ease easeType = Ease.OutQuad;

    [SerializeField] private bool snapping = false;
    [SerializeField] private int vibrato = 10;
    [SerializeField] private float elasticity = 1f;
    [SerializeField] private bool fadeCanvasGroup = false;
    
    private Tween currentTween;
    
    /// <summary>
    /// Creates and starts the appropriate tween based on the configured settings
    /// </summary>
    protected override void OnExecute()
    {
        if (target == null) 
        {
            Complete();
            return;
        }

        currentTween = CreateTween()
            .SetEase(easeType)
            .OnComplete(() => Complete());
    }
    
    private Tween CreateTween()
    {
        switch (tweenType)
        {
            case TweenType.Move:
                return CreateMoveTween();
            case TweenType.Rotate:
                return CreateRotateTween();
            case TweenType.Scale:
                return CreateScaleTween();
            case TweenType.Fade:
                return CreateFadeTween();
            default:
                return null;
        }
    }
    
    private Tween CreateMoveTween()
    {
        switch (tweenMode)
        {
            case TweenMode.To:
                return target.DOMove(value, duration, snapping);
            case TweenMode.From:
                return target.DOMove(value, duration, snapping).From();
            case TweenMode.Punch:
                return target.DOPunchPosition(value, duration, vibrato, elasticity, snapping);
            case TweenMode.Shake:
                return target.DOShakePosition(duration, value, vibrato, elasticity, snapping);
            default:
                return null;
        }
    }
    
    private Tween CreateRotateTween()
    {
        switch (tweenMode)
        {
            case TweenMode.To:
                return target.DORotate(value, duration, RotateMode.FastBeyond360);
            case TweenMode.From:
                return target.DORotate(value, duration, RotateMode.FastBeyond360).From();
            case TweenMode.Punch:
                return target.DOPunchRotation(value, duration, vibrato, elasticity);
            case TweenMode.Shake:
                return target.DOShakeRotation(duration, value, vibrato, elasticity);
            default:
                return null;
        }
    }
    
    private Tween CreateScaleTween()
    {
        switch (tweenMode)
        {
            case TweenMode.To:
                return target.DOScale(value, duration);
            case TweenMode.From:
                return target.DOScale(value, duration).From();
            case TweenMode.Punch:
                return target.DOPunchScale(value, duration, vibrato, elasticity);
            case TweenMode.Shake:
                return target.DOShakeScale(duration, value, vibrato, elasticity);
            default:
                return null;
        }
    }
    
    private Tween CreateFadeTween()
    {
        if (fadeCanvasGroup)
        {
            var canvasGroup = target.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                switch (tweenMode)
                {
                    case TweenMode.To:
                        return canvasGroup.DOFade(value.x, duration);
                    case TweenMode.From:
                        return canvasGroup.DOFade(value.x, duration).From();
                    default:
                        return null;
                }
            }
        }
        
        return null;
    }
} 