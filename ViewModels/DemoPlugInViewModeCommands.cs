using EAS.LeegooBuilder.Client.Common.ToolsAndUtilities.Extensions;
using EAS.LeegooBuilder.Client.Common.ToolsAndUtilities.ViewModels;
using EAS.LeegooBuilder.Client.Common.ToolsAndUtilities.Views.Helpers;
using EAS.LeegooBuilder.Client.GUI.Modules.MainModule.Models;
using EAS.LeegooBuilder.Common.CommonTypes.EventTypes;
using EAS.LeegooBuilder.Common.CommonTypes.Helpers;
using EAS.LeegooBuilder.Server.DataAccess.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EAS.LeegooBuilder.Common.CommonTypes.Constants;
using EAS.LeegooBuilder.Server.DataAccess.Core.Elements;


namespace DemoPlugIn.ViewModels
{
    partial class DemoPlugInViewModel
    {

        private void ExecuteDoSomething()
        {
            //ProjectAndConfigurationModel.BeginUpdateConfiguration();

            // Neue Komponente einfügen
            var newTreeItem = ProjectAndConfigurationModel.CreateConfigurationItemFromElement(
                        Guid.Parse("{6552C0AE-FCE3-E511-8B07-005056AB4E2A}"),  //"@_SCKCN",
                        SelectedConfigurationTreeItem.Value.ComponentID,
                        TreeStructureItemInsertMode.AddFirstChild);
            ProjectAndConfigurationModel.EndUpdateConfiguration();


            // Merkmal buchen
            var localAttributes = ProjectAndConfigurationModel.GetLocalAttributes(newTreeItem.Value.ComponentID, newTreeItem.Value.Element.InternalElementID, User.CurrentUser.LBUser.Language);
            var localAttributeInfo = localAttributes.FirstOrDefault(item => item.AttributeName == "LA_PO_01");
            if (localAttributeInfo != null)
            {
                localAttributeInfo.DataValue = "23";
                ProjectAndConfigurationModel.SetLocalAttribute(newTreeItem.Value.ComponentID, localAttributeInfo);
            }




            //NotImplemented();
        }

        private bool CanExecuteDoSomething(out string errorMessage)
        {
            errorMessage = string.Empty;
            return true;
        }


        private void ExecuteDoSomethingElse()
        {
            var filterQuery = new FilterElement
            {
                Language = User.CurrentUser.LBUser.Language,
                FilterMode = ElementsFilterMode.Advanced,
                NotCheckMaxRowNumber = true,
                AdvancedWhere = "BGRNR like '220%'"
            };

            var elements = ProjectAndConfigurationModel.GetElementsWithFilter(filterQuery);
        }


        private bool CanExecuteDoSomethingElse(out string errorMessage)
        {
            errorMessage = string.Empty;
            return true;
        }


        private void ExecuteDoFilterElements()
        {
            MouseHelper.WaitIdle();
            var filter = new FilterElement { CatalogueID = "120", ElementID = "PE" };
            var elements = ProjectAndConfigurationModel.GetElementsWithFilter(filter);

            MessageBox.Show(string.Format("{0} elements found", elements.Count));
        }


        private bool CanExecuteDoFilterElements(out string errorMessage)
        {
            errorMessage = string.Empty;
            return true;
        }


        private void ExecuteShowProposalId()
        {
            MessageBox.Show(ProjectAndConfigurationModel.SelectedProposal.ProposalID);
        }

        private bool CanExecuteShowProposalId(out string errorMessage)
        {
            if (ProjectAndConfigurationModel.SelectedProposal == null)
            {
                errorMessage = "No proposal selected";
                return false;
            }

            errorMessage = string.Empty;
            return true;
        }


        private void ExecuteSelectSomething()
        {
            var sqlResult = (string)ProjectAndConfigurationModel.SqlExecuteScalar("select VALUE from SYS_SETTINGS where PARAMETER='KURZBEZ'");
            MessageBox.Show(sqlResult);
        }



        private bool CanExecuteSelectSomething(out string errorMessage)
        {
            errorMessage = string.Empty;
            return true;
        }


        private void ExecuteShowBusiIndicator()
        {
            Task.Factory.ExecuteAndWaitNonBlocking(() =>
                                                   {
                                                       StartProgressBar("Processing something...");

                                                       // Do something...
                                                       Thread.Sleep(5000);

                                                       EndProgressBar();
                                                   });

        }



        private bool CanExecuteShowBusiIndicator(out string errorMessage)
        {
            errorMessage = string.Empty;
            return true;
        }



        private void ExecuteExecuteScript()
        {
            var scriptName = "HelloWorld";
            var script = ProjectAndConfigurationModel.LoadScriptByName(scriptName);
            if (script == null)
                MessageBox.Show(string.Format("Scipt '{0}' not found!", "HelloWorld"), MessageBoxType.Error);

            else
            {
                var args = new CustomScriptArgs(ProjectAndConfigurationModel.SelectedProposal);
                args.CurrentConfigurationItemId = (SelectedConfigurationTreeItem != null) ? SelectedConfigurationTreeItem.Value.ComponentID : Guid.Empty;

                // pass some custom parameters to the script
                args.CustomArgs = new object[] { "abc", 123 };


                MessageBox.Show(string.Format("Going to start Script '{0}'", script.Name));

                var scriptResult = ProjectAndConfigurationModel.ExecuteCustomScript(script.ScriptID, args);

                var resultMessage = string.Format("Script '{0}' was executed.", script.Name);
                if (scriptResult != null)
                    resultMessage = string.Format("{0}\nResult is {1}", resultMessage, scriptResult.ToString());
                MessageBox.Show(resultMessage);

            }

        }


        private bool CanExecuteExecuteScript(out string errorMessage)
        {
            errorMessage = string.Empty;
            return true;
        }



        private void ExecuteGetCustomDefinitionsOfCompany2()
        {
            if (ProjectAndConfigurationModel.SelectedProposal?.Company2ID != null)
            {
                var company2Id = ProjectAndConfigurationModel.SelectedProposal.Company2ID.Value;
                var language = User.CurrentUser.LBUser.Language;

                var customDefinitionValueInfos = ProjectAndConfigurationModel.GetCustomDefinitionValuesInfos(CustomDefinitionTableType.Company, company2Id, language);

                MessageBox.Show($"{customDefinitionValueInfos.Count} CustomDefinitions wurden geladen.");
            }


        }

        private bool CanExecuteGetCustomDefinitionsOfCompany2(out string errorMessage)
        {
            if (ProjectAndConfigurationModel.SelectedProposal == null)
            {
                errorMessage = "No proposal selected!";
                return false;
            }

            if (ProjectAndConfigurationModel.SelectedProposal?.Company2ID == null)
            {
                errorMessage = "No company2 set!";
                return false;
            }

            errorMessage = string.Empty;
            return true;
        }

    }
}
