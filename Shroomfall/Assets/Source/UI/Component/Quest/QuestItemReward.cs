using Assets.Source.Data;
using Contract.DTO.Runtime.MetaDomain;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Source.UI.Component.Quest
{
    public class QuestItemReward : MonoBehaviour
    {
        #region Attributes
        [Header("References")]
        [SerializeField] private Image itemIcon;
        [SerializeField] private TMP_Text rewardText;

        [Header("Data")]
        [SerializeField] private ItemCatalogSO itemCatalog;
        #endregion

        #region Methods
        public void Render(
            GachaRewardEntryDTO entry,
            float totalWeight)
        {
            itemIcon.sprite = itemCatalog.TryGet(entry.ItemDefinitionId, out ItemAsset asset)
                ? asset.icon
                : null;

            // Rate is the entry's share of the gacha pool
            float rate = totalWeight > 0 ? entry.Weight / totalWeight * 100f : 0f;
            rewardText.text = $"x{entry.Amount} ({rate:0.#}%)";
        }

        // Shared by QuestItem and QuestProgressItem: rebuilds every reward entry under content
        public static void RenderAll(
            QuestRewardConfig config,
            QuestItemReward prefab,
            Transform content,
            List<QuestItemReward> spawned)
        {
            Clear(spawned);

            if (config?.Entries == null)
                return;

            float totalWeight = config.Entries.Sum(e => e.Weight);
            foreach (GachaRewardEntryDTO entry in config.Entries)
            {
                QuestItemReward reward = Instantiate(prefab, content);
                reward.Render(entry, totalWeight);
                spawned.Add(reward);
            }
        }

        public static void Clear(
            List<QuestItemReward> spawned)
        {
            foreach (QuestItemReward reward in spawned)
            {
                if (reward != null)
                    Destroy(reward.gameObject);
            }

            spawned.Clear();
        }
        #endregion
    }
}