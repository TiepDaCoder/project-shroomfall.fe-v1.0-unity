using Assets.Utilities;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class JoinCombatView : MonoBehaviour
{
    #region Attributes
    [Header("Button")]
    [SerializeField] private Button backButton;
    [SerializeField] private TextButton hostButton;
    [SerializeField] private TextButton joinButton;

    [Header("Label")]
    [SerializeField] private TMP_Text viewLabel;
    //[SerializeField] private TMP_Text codeLabel;

    [Header("Placeholders")]
    [SerializeField] private TMP_Text codePlaceholder;

    [Header("Text Fields")]
    [SerializeField] private TMP_InputField codeTextField;
    #endregion

    #region Properties
    public event Action OnBackClicked;
    public event Action OnHostClicked;
    public event Action OnJoinClicked;
    public event Action<string> OnCodeChanged;
    #endregion

    #region Methods
    private void Awake()
    {
        // Buttons
        backButton.onClick.AddListener(() => { OnBackClicked?.Invoke(); });
        hostButton.onClick.AddListener(() => { OnHostClicked?.Invoke(); });
        joinButton.onClick.AddListener(() => { OnJoinClicked?.Invoke(); });

        // Text Fields
        codeTextField.onValueChanged.AddListener((string code) => { OnCodeChanged?.Invoke(code); });
    }

    public void SetVisible(
        bool visible)
    {
        gameObject.SetActive(visible);
        if (visible) RefreshLocalizedText();
    }

    public void SetInteractable(
        bool interactable)
    {
        backButton.interactable = interactable;
        hostButton.interactable = interactable;
        joinButton.interactable = interactable;
    }

    private void RefreshLocalizedText()
    {
        // Buttons
        hostButton.SetText(UILocalizationTable.Get("join-combat.btn-host"));
        joinButton.SetText(UILocalizationTable.Get("join-combat.btn-join"));

        // Labels
        viewLabel.SetText(UILocalizationTable.Get("join-combat.label-join"));
        //codeLabel.SetText(UILocalizationTable.Get("join-combat.label-code"));

        // Placeholders
        codePlaceholder.SetText(UILocalizationTable.Get("join-combat.placeholder-code"));
    }
    #endregion
}