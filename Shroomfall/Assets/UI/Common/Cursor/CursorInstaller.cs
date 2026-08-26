using Assets.Services;
using Assets.UI.Common.Cursor;
using Assets.Utilities;
using System.Collections;
using UnityEngine;

public class CursorInstaller : Installer
{
    #region Attributes
    [SerializeField]
    private CursorView cursorView;
    private CursorPresenter cursorPresenter;

    private UIService uiService;
    #endregion

    #region Properties
    public override string StepName
    {
        get { return UILocalizationTable.Get("cursor-binder.step-name"); }
    }
    #endregion

    #region Methods
    public override IEnumerator BindAllServices()
    {
        yield return BindWhenReady<UIService>(ui => { uiService = ui; });

        // Resolve dependencies
        cursorPresenter = new CursorPresenter(
            uiService,
            cursorView);

        // Bind targets
        CursorTarget.OnTargetEnabled += cursorPresenter.BindTarget;
        CursorTarget.OnTargetDisabled += cursorPresenter.UnbindTarget;
        foreach (var target in FindObjectsByType<CursorTarget>(FindObjectsSortMode.None))
        {
            if (target.isActiveAndEnabled)
            {
                cursorPresenter.BindTarget(target);
            }
        }
    }

    private void OnDestroy()
    {
        cursorPresenter?.Dispose();
    }
    #endregion
}