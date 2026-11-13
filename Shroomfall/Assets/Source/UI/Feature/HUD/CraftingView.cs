using Assets.Source.Core;
using Assets.Source.Service;
using Assets.Source.UI.Component.Crafting;
using Contract.DTO.Definition.MetaDomain;
using Contract.DTO.Feature.Design.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Source.UI.Feature.HUD
{
    public class CraftingView : MonoBehaviour, IHUDView
    {
        #region Attributes
        [Header("Root")]
        [SerializeField] private GameObject root;

        [Header("References")]
        [SerializeField] private Transform content;
        [SerializeField] private RecipeContainer recipePrefab;

        [Header("Pagination")]
        [SerializeField, Min(1)] private int pageSize = 6;
        [SerializeField] private Button previousButton;
        [SerializeField] private Button nextButton;
        [SerializeField] private TMP_Text pageText;

        [Header("Search")]
        [SerializeField] private TMP_InputField searchTextField;

        private DefinitionService definitionService;

        private readonly List<ItemDefinitionDTO> recipes = new();
        private readonly List<ItemDefinitionDTO> filteredRecipes = new();
        private readonly List<RecipeContainer> recipeContainers = new();
        private int currentPage;
        #endregion

        #region Properties
        #endregion

        #region Methods
        private void Awake()
        {
            definitionService = ServiceProvider.Get<DefinitionService>();

            // Definition Service
            definitionService.OnChanged += RefreshRecipes;

            // Pagination
            previousButton.onClick.AddListener(PreviousPage);
            nextButton.onClick.AddListener(NextPage);

            // Search
            searchTextField.onValueChanged.AddListener(OnSearchChanged);

            RefreshRecipes();
        }

        private void OnDestroy()
        {
            // Definition Service
            definitionService.OnChanged -= RefreshRecipes;

            // Pagination
            previousButton.onClick.RemoveListener(PreviousPage);
            nextButton.onClick.RemoveListener(NextPage);

            // Search
            searchTextField.onValueChanged.RemoveListener(OnSearchChanged);

            ClearRecipes();
        }

        #region HUD Action
        public void Show()
        {
            root.SetActive(true);
            RenderPage();
        }

        public void Hide()
        {
            root.SetActive(false);
        }
        #endregion

        private void OnSearchChanged(
            string searchText)
        {
            currentPage = 0;
            ApplySearch();
        }

        private void ApplySearch()
        {
            filteredRecipes.Clear();

            string searchText = searchTextField != null ? searchTextField.text.Trim() : string.Empty;
            if (string.IsNullOrEmpty(searchText))
            {
                filteredRecipes.AddRange(recipes);
            }
            else
            {
                filteredRecipes.AddRange(recipes.Where(item =>
                {
                    string localizedName = GetLocalizedItemName(item);
                    return localizedName.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0;
                }));
            }

            int totalPages = GetTotalPages();
            currentPage = Mathf.Clamp(currentPage, 0, Mathf.Max(0, totalPages - 1));
            RenderPage();
        }

        private string GetLocalizedItemName(
            ItemDefinitionDTO item)
        {
            if (item?.Presentation?.LocalizedText == null)
                return item?.Id ?? string.Empty;

            string nameKey = item.Presentation.LocalizedText.NameKey;
            if (string.IsNullOrEmpty(nameKey))
                return item.Id;

            string localizedName = definitionService.GetLocalizedText(nameKey);
            return string.IsNullOrEmpty(localizedName) ? item.Id : localizedName;
        }
        #endregion

        #region Recipe Management
        public void RefreshRecipes()
        {
            recipes.Clear();

            DefinitionSnapshotDTO snapshot = definitionService.Snapshot;
            if (snapshot?.Items != null)
            {
                recipes.AddRange(snapshot.Items
                    .Where(item => item != null && item.RequiredMaterials != null && item.RequiredMaterials.Count > 0)
                    .OrderBy(item => item.Id));
            }

            ApplySearch();
        }

        private void ClearRecipes()
        {
            foreach (RecipeContainer container in recipeContainers)
                if (container != null)
                    Destroy(container.gameObject);

            recipeContainers.Clear();
        }
        #endregion

        #region Pagination
        private void RenderPage()
        {
            ClearRecipes();

            int safePageSize = Mathf.Max(1, pageSize);
            int startIndex = currentPage * safePageSize;
            int endIndex = Mathf.Min(startIndex + safePageSize, filteredRecipes.Count);

            for (int i = startIndex; i < endIndex; i++)
            {
                RecipeContainer container = Instantiate(recipePrefab, content);
                container.Render(root.GetComponent<Transform>(), filteredRecipes[i]);
                recipeContainers.Add(container);
            }

            UpdatePagination();
        }

        private void UpdatePagination()
        {
            int totalPages = GetTotalPages();

            if (previousButton != null)
                previousButton.interactable = currentPage > 0;

            if (nextButton != null)
                nextButton.interactable = currentPage < totalPages - 1;

            if (pageText != null)
                pageText.text = totalPages == 0 ? "0 / 0" : $"{currentPage + 1} / {totalPages}";
        }

        private int GetTotalPages()
        {
            int safePageSize = Mathf.Max(1, pageSize);
            return Mathf.CeilToInt((float)filteredRecipes.Count / safePageSize);
        }

        private void PreviousPage()
        {
            if (currentPage <= 0)
                return;

            currentPage--;
            RenderPage();
        }

        private void NextPage()
        {
            if (currentPage >= GetTotalPages() - 1)
                return;

            currentPage++;
            RenderPage();
        }
        #endregion
    }
}