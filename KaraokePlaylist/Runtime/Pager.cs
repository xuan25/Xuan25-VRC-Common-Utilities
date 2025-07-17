
using System;
using Playlist;
using TMPro;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

namespace Playlist
{
    
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class Pager : UdonSharpBehaviour
    {
        public Button nextButton;
        public Button previousButton;
        public TextMeshProUGUI textCurrentPage;
        public TextMeshProUGUI textTotalPages;

        private PlaylistIndexer indexer;
        private int currentPage = 0;
        private int itemsPerPage = 10;
        private int totalItems = 0;
        private int totalPages = 0;

        private bool isConfigured = false;

        void Start()
        {
            Reset();
        }

        public void Setup(PlaylistIndexer indexer)
        {
            this.indexer = indexer;
        }
        
        public void Config(int itemsPerPage, int totalItems)
        {
            this.itemsPerPage = itemsPerPage;
            this.totalItems = totalItems;
            this.totalPages = Mathf.CeilToInt((float)totalItems / itemsPerPage);

            if (currentPage >= totalPages)
            {
                currentPage = totalPages - 1;
            }

            Debug.Log($"Pager Configured: itemsPerPage: {itemsPerPage}, totalItems: {totalItems}, totalPages: {totalPages}");

            isConfigured = true;

            UpdateInfoDisplay();
        }

        public void Reset()
        {
            currentPage = 0;
            totalItems = 0;
            totalPages = 0;

            isConfigured = false;

            UpdateInfoDisplay();
        }

        public void NextPage()
        {
            if (currentPage < totalPages - 1)
            {
                currentPage++;
            }

            OnCurrentPageChanged();
        }

        public void PreviousPage()
        {
            if (currentPage > 0)
            {
                currentPage--;
            }

            OnCurrentPageChanged();
        }
        
        private void OnCurrentPageChanged()
        {
            int startIndex = currentPage * itemsPerPage;
            Debug.Log($"Current Page: {currentPage}, Start Index: {startIndex}");

            indexer.SetItemDisplayOffset(startIndex);

            UpdateInfoDisplay();
        }

        private void UpdateInfoDisplay()
        {
            if (!isConfigured)
            {
                textCurrentPage.text = "--";
                textTotalPages.text = "--";
                nextButton.interactable = false;
                previousButton.interactable = false;
                return;
            }
            textCurrentPage.text = (currentPage + 1).ToString();
            textTotalPages.text = totalPages.ToString();
            nextButton.interactable = currentPage < totalPages - 1;
            previousButton.interactable = currentPage > 0;
        }
    }

}