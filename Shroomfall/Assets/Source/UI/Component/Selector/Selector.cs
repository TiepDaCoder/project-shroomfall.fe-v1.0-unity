using Assets.Enum;
using Assets.UI.Models;
using System;
using System.Collections.Generic;
using UnityEngine;

public class Selector : MonoBehaviour
{
    #region Attributes
    [Header("Configuration")]
    [SerializeField] private SelectorItem prefab;
    [SerializeField] private RectTransform root;
    [SerializeField] private SelectionMode selectionMode = SelectionMode.Single;

    private SelectorItem singleSelectedView;
    private readonly HashSet<SelectorItem> multipleSelectedViews = new();
    private readonly List<SelectorItem> spawned = new();
    #endregion

    #region Properties
    public SelectionMode Mode
    {
        get => selectionMode;
        set => selectionMode = value;
    }
    public event Action<IconModel, bool> OnItemToggled;
    #endregion

    #region Methods
    public void Bind(
        List<IconModel> items,
        string currentValueId = null)
    {
        // Cleanup existing instances
        foreach (var v in spawned)
            if (v != null) Destroy(v.gameObject);

        spawned.Clear();
        singleSelectedView = null;
        multipleSelectedViews.Clear();

        // Spawn dynamic items
        foreach (var item in items)
        {
            var view = Instantiate(prefab, root);
            view.Bind(item);
            spawned.Add(view);

            // Bind click hook
            view.OnSelect += () => HandleSelect(view, item);

            // Direct ID matching verification check instead of invoking a delegate
            if (!string.IsNullOrEmpty(currentValueId) && item.Id == currentValueId)
            {
                if (selectionMode == SelectionMode.Single)
                {
                    if (singleSelectedView != null)
                        singleSelectedView.SetSelected(false);

                    singleSelectedView = view;
                    singleSelectedView.SetSelected(true);
                }
                else
                {
                    multipleSelectedViews.Add(view);
                    view.SetSelected(true);
                }
            }
        }
    }

    private void HandleSelect(
        SelectorItem view,
        IconModel model)
    {
        if (selectionMode == SelectionMode.Single)
        {
            HandleSingleSelect(view, model);
        }
        else
        {
            HandleMultipleSelect(view, model);
        }
    }

    private void HandleSingleSelect(
        SelectorItem view,
        IconModel model)
    {
        if (singleSelectedView == view)
            return;

        // Turn off previous selection
        if (singleSelectedView != null)
        {
            singleSelectedView.SetSelected(false);
        }

        // Turn on new selection
        singleSelectedView = view;
        singleSelectedView.SetSelected(true);

        OnItemToggled?.Invoke(model, true);
    }

    private void HandleMultipleSelect(
        SelectorItem view,
        IconModel model)
    {
        bool isNowSelected;

        if (multipleSelectedViews.Contains(view))
        {
            multipleSelectedViews.Remove(view);
            view.SetSelected(false);
            isNowSelected = false;
        }
        else
        {
            multipleSelectedViews.Add(view);
            view.SetSelected(true);
            isNowSelected = true;
        }

        OnItemToggled?.Invoke(model, isNowSelected);
    }
    #endregion
}