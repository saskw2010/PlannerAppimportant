using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using System.Net.Http;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.Web.Virtualization;
using Microsoft.AspNetCore.Components.WebAssembly.Http;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.JSInterop;
using PlannerApp;
using PlannerApp.Shared;
using PlannerApp.Components;
using MudBlazor;
using Blazored.FluentValidation;
using PlaneerApp.Client.Services.Interfaces;
using PlaneerApp.Client.Services.Exceptions;
using PlannerApp.Shared.Models;
using AKSoftware.Blazor.Utilities;

namespace PlannerApp.Components
{
    public partial class PlansList
    {

        [Inject]
        public IPlansService PlansService { get; set; }

        [Inject]
        public NavigationManager Navigation { get; set; }

        [Inject]
        public IDialogService DialogService {  get; set; }

        [CascadingParameter]
        public Error Error { get; set; }

        private bool _isBusy = false;
        private string _errorMessage = string.Empty;
        private int _pageNumber = 1;
        private int _pageSize = 10;
        private string _query = string.Empty;
        private int _totalPages = 1;

        private List<PlanSummary> _plans = new();

        private async Task<PagedList<PlanSummary>> GetPlansAsync(string query = "", int pageNumber = 1, int pageSize = 10)
        {
            _isBusy = true;
            try
            {
                var result = await PlansService.GetPlansAsync(query, pageNumber, pageSize);
                _plans = result.Value.Records.ToList();
                _pageNumber = result.Value.Page;
                _pageSize = result.Value.PageSize;
                _totalPages = result.Value.TotalPages;
                return result.Value;
            }
            catch (ApiException ex)
            {
                _errorMessage = ex.ApiErrorResponse.Message;
            }
            catch (Exception ex)
            {
                Error.HandleError(ex);
            }
            _isBusy = false;
            return null;
        }

        #region View Toggler
        private bool _isCardsViewEnabled = true;

        private void SetCardsView()
        {
            _isCardsViewEnabled = true;
        }

        private void SetTableView()
        {
            _isCardsViewEnabled = false;
        }

        #endregion

        #region Edit
        private void EditPlan(PlanSummary plan)
        {
            Navigation.NavigateTo($"/plans/form/{plan.Id}");
        }
        #endregion

        #region Delete
        private async Task DeletePlanAsync(PlanSummary plan)
        {
            var parameters = new DialogParameters();
            parameters.Add("ContentText", $"Do you really want to delete the plan '{plan.Title}'?");
            parameters.Add("ButtonText", "Delete");
            parameters.Add("Color", Color.Error);

            var options = new DialogOptions() { CloseButton = true, MaxWidth = MaxWidth.ExtraSmall };

            var dialog = DialogService.Show<ConfirmationDialog>("Delete", parameters, options);
            var confirmationResult = await dialog.Result; 

            if (!confirmationResult.Cancelled)
            {
                // Confirmed to delete
                try
                {
                    await PlansService.DeleteAsync(plan.Id);

                    // Send a message about the deleted plan
                    MessagingCenter.Send(this, "plan_deleted", plan);
                }
                catch (ApiException ex)
                {
                    // TODO: Log this error 
                }
                catch (Exception ex)
                {
                    // TODO: Log this error 
                }
                
            }
        }
        #endregion

        #region View 
        private void ViewPlan(PlanSummary plan)
        {
            var parameters = new DialogParameters();
            parameters.Add("PlanId", plan.Id);

            var options = new DialogOptions() { CloseButton = true, MaxWidth = MaxWidth.Medium };

            var dialog = DialogService.Show<PlanDetailsDialog>("Details", parameters, options);
        }
        #endregion 
    }
}