﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using EAS.DevExpressGenericDialogs.Dialogs;
using EAS.DevExpressGenericDialogs.Model;
using EAS.LeegooBuilder.Client.Common.ToolsAndUtilities.Events;
using EAS.LeegooBuilder.Client.Common.ToolsAndUtilities.Extensions;
using EAS.LeegooBuilder.Client.Common.ToolsAndUtilities.Interfaces;
using EAS.LeegooBuilder.Client.Common.ToolsAndUtilities.ViewModels;
using EAS.LeegooBuilder.Client.Common.ToolsAndUtilities.Views.Helpers;
using EAS.LeegooBuilder.Client.GUI.Modules.MainModule.Models;
using EAS.LeegooBuilder.Client.GUI.Modules.MainModule.ViewModels;
using EAS.LeegooBuilder.Client.GUI.Modules.Plugin.Helper;
using EAS.LeegooBuilder.Common.CommonTypes.Constants;
using EAS.LeegooBuilder.Common.CommonTypes.EventTypes;
using EAS.LeegooBuilder.Common.CommonTypes.EventTypes.Programmereignismethoden;
using EAS.LeegooBuilder.Common.CommonTypes.Helpers;
using EAS.LeegooBuilder.Common.CommonTypes.Models;
using EAS.LeegooBuilder.Common.CommonTypes.Parameterclasses;
using EAS.LeegooBuilder.Common.CommonTypes.ProposalHelper;
using EAS.LeegooBuilder.Common.Security.Definitions;
using EAS.LeegooBuilder.Common.Security.SecurityActions;
using EAS.LeegooBuilder.Server.DataAccess.Core;
using EAS.LeegooBuilder.Server.DataAccess.Core.Attributes;
using EAS.LeegooBuilder.Server.DataAccess.Core.Configuration;
using EAS.LeegooBuilder.Server.DataAccess.Core.Elements;
using EAS.LeegooBuilder.Server.DataAccess.Core.Proposals;
using Microsoft.Win32;
using MessageBox = System.Windows.MessageBox;
using static System.FormattableString;
using MessageBoxButton = EAS.LeegooBuilder.Common.CommonTypes.Helpers.MessageBoxButton;
using MessageBoxImage = EAS.LeegooBuilder.Common.CommonTypes.Helpers.MessageBoxImage;
using MessageBoxResult = EAS.LeegooBuilder.Common.CommonTypes.Helpers.MessageBoxResult;

namespace EAS.LeegooBuilder.Client.GUI.Modules.Plugin.ViewModels
{
    enum MoveDirection
    {
        Up, Down
    }
    
    
    partial class PluginViewModel
    {
        private ProjectsAndProposalsViewModel ProjectsAndProposalsViewModel => _projectsAndProposalsViewModel ?? (_projectsAndProposalsViewModel = serviceLocator.GetInstance<ProjectsAndProposalsViewModel>());

        private ProjectsAndProposalsViewModel _projectsAndProposalsViewModel;


        private void ExecuteDoSomething()
        {
          
            ProjectAndConfigurationModel.BeginUpdateConfiguration();

            // Insert a new ConfigurationItem
            var parameters = new CreateConfigurationItemParameters(
                Guid.Parse("{6552C0AE-FCE3-E511-8B07-005056AB4E2A}"),  //"@_SCKCN",
                SelectedConfigurationTreeItem.Value.ComponentID,
                TreeStructureItemInsertMode.AddFirstChild);

            var newTreeItem = ProjectAndConfigurationModel.CreateConfigurationItemFromElement(parameters, out var errorMessage);




            ProjectAndConfigurationModel.EndUpdateConfiguration();


            // Set a LocalAttribute
            var localAttributes = ProjectAndConfigurationModel.GetLocalAttributes(newTreeItem.Value.ComponentID, newTreeItem.Value.Element.InternalElementID, User.CurrentUser.LBUser.Language);
            var localAttributeInfo = localAttributes.FirstOrDefault(item => item.AttributeName == "LA_PO_01");
            if (localAttributeInfo != null)
            {
                localAttributeInfo.DataValue = "23";
                ProjectAndConfigurationModel.SetLocalAttribute(newTreeItem.Value.ComponentID, localAttributeInfo);
            }
            

            // Set a GlobalAttribute
            //ProjectAndConfigurationModel.LoadGlobalAttributes(SelectedConfigurationTreeItem.Value);
            var globalAttributes = SelectedConfigurationTreeItem?.Value.GlobalAttributes;
            MessageBox.Show($"global attributes = {globalAttributes.Count}");
            var globalAttribute = globalAttributes.FirstOrDefault(item => item.Name == "COOLINGWATER");

            if (globalAttribute != null)
            {
                globalAttribute.AttributeValue = "100";
                globalAttribute.ValueSource = 9; // manuelle Eingabe

                ProjectAndConfigurationModel.SaveGlobalAttributes(new List<GlobalAttribute>() { globalAttribute });
            }
            
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
            MessageBox.Show(ProjectAndConfigurationModel.SelectedProposal.ProposalID + Environment.NewLine +
                ProjectAndConfigurationModel.SelectedProposal.IsProposalViewOnlyByUser);
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


        private void ExecuteCreateProposal()
        {

            var creationMode = NewProposalCreationMode.NewProposalWithoutReference;
            Proposal sourceProposal = null; // not needed (only when creating an appendx or a copy from template)
            var destinationProject = ProjectAndConfigurationModel.GetProject(ProjectAndConfigurationModel.SelectedProjectInfo.InternalProjectID);


            var createNewProposalIdArgs = new CreateNewProposalIdArgs { CreationMode = creationMode, SourceProposal = sourceProposal, DestinationProject = destinationProject };
            var newProposalId = ProjectAndConfigurationModel.GenerateNewProposalId(creationMode, destinationProject.InternalProjectID, sourceProposal?.InternalProposalID ?? Guid.Empty, out var proposalIdMainPart, out var proposalIdAppendixPart, out var editableDefinition);

            var newProposal = ProjectAndConfigurationModel.CreateNewProposal(createNewProposalIdArgs,
                newProposalId,
                proposalIdMainPart,
                proposalIdAppendixPart,
                User.CurrentUser.LBUser.UserID,
                new ClientDetails(true),
                out var errorMessageInfo);


            // Create initial Configuration
            var mainConfigurations = ProjectAndConfigurationModel.LoadConstructionKitHeaders(User.CurrentUser.LBUser.Language, MasterStructureType.MainConfiguration);
            var usedMainConfiguration = mainConfigurations?.First(); // hier in geeigneter Weise einen Eintrag ermitteln
            ProjectAndConfigurationModel.InitializeConfigurationFromConstructionKit(newProposal, usedMainConfiguration);

            //ProjectAndConfigurationModel.SaveProposal2(newProposal);

            // Update proposal list and select the new item
            ProjectAndConfigurationModel.MakeProposalVisible(newProposal);
        }


        private void ExecuteCreateAppendix()
        {
            var creationMode = NewProposalCreationMode.NewAppendix;
            Proposal sourceProposal = ProjectAndConfigurationModel.SelectedProposal;
            var destinationProject =  ProjectAndConfigurationModel.GetProject(ProjectAndConfigurationModel.SelectedProjectInfo.InternalProjectID);
            

            var createNewProposalIdArgs = new CreateNewProposalIdArgs { CreationMode = creationMode, SourceProposal = sourceProposal, DestinationProject = destinationProject };
            var newProposalId = ProjectAndConfigurationModel.GenerateNewProposalId(creationMode, destinationProject.InternalProjectID, sourceProposal?.InternalProposalID ?? Guid.Empty, out var proposalIdMainPart, out var proposalIdAppendixPart, out var editableDefinition);

            var newProposal = ProjectAndConfigurationModel.CreateNewProposal(createNewProposalIdArgs,
                newProposalId,
                proposalIdMainPart,
                proposalIdAppendixPart,
                User.CurrentUser.LBUser.UserID,
                new ClientDetails(true),
                out var errorMessageInfo);

            
            //ProjectsAndProposalsViewModel.UpdateProposalList(EAS.LeegooBuilder.Common.CommonTypes.ProposalHelper.ProposalGridUpdateTypeEnumsClass.ProposalGridUpdateType.ClearAll);

            ProjectAndConfigurationModel.SelectedProposal = newProposal;
            ProjectsAndProposalsViewModel.UpdateProposalList(EAS.LeegooBuilder.Common.CommonTypes.ProposalHelper.ProposalGridUpdateTypeEnumsClass.ProposalGridUpdateType.Insert);
            //ProjectsAndProposalsViewModel.UpdateProposalList(EAS.LeegooBuilder.Common.CommonTypes.ProposalHelper.ProposalGridUpdateTypeEnumsClass.ProposalGridUpdateType.LoadAll);
            //ProjectsAndProposalsViewModel.UpdateProposalGridPropertyAndValueList(ProposalGridUpdateTypeEnumsClass.ProposalGridUpdateType.LoadAll);
            //ProjectsAndProposalsViewModel.ProposalGridPropertyAndValueList = ProjectsAndProposalsViewModel.LoadProposalGridPropertyAndValueList();
            
            
            /*ProposalGridPropertyAndValueListSettings proposalGridPropertyAndValueListSettings = new ProposalGridPropertyAndValueListSettings()
            {
                CustomDefinitionInfos = _listOfCustomDefinitionInfos,
                SystemViewColumnTemplateItemList = _systemViewColumnTemplateItemList,
                DictionarySystemProposalViewColumnTemplates = _dictionarySystemProposalViewColumnTemplates,
                SysCodes = ProjectAndConfigurationModel.SysCodes,
                LoginCultureInfoName = ProjectAndConfigurationModel.LoginCultureInfo.Name
            };            
            ProjectsAndProposalsViewModel.ProposalGridPropertyAndValueList = ProjectAndConfigurationModel?.GetProposalsGridPropertyAndValueList(proposalGridPropertyAndValueListSettings);*/
        }
        

        private bool CanExecuteCreateProposal(out string errorMessage)
        {
            errorMessage = string.Empty;
            return true;
        }


        private void ExecuteSetProposalCustomProperty()
        {

            var newValue = "test123";

            var proposal = ProjectAndConfigurationModel.SelectedProposal;
            var customDefinitionValuesInfos = ProjectAndConfigurationModel.GetCustomDefinitionValuesInfos(CustomDefinitionTableType.Proposal, proposal.InternalProposalID, "en-GB");
            var customDefinitionValueInfo = customDefinitionValuesInfos.FirstOrDefault(item => item.CustomFieldName.Equals("OpportunityId", StringComparison.CurrentCultureIgnoreCase));


            if ((customDefinitionValueInfo != null) && (customDefinitionValueInfo.StringValue != newValue))
            {
                customDefinitionValueInfo.StringValue = newValue;
                ProjectAndConfigurationModel.SaveCustomDefinitionValueInfos(customDefinitionValueInfo);
            }
        }


        private bool CanExecuteSetProposalCustomProperty(out string errorMessage)
        {
            if (ProjectAndConfigurationModel.SelectedProposal == null)
            {
                errorMessage = "No proposal selected";
                return false;
            }

            errorMessage = string.Empty;
            return true;
        }


        private void ExecuteToggleLockProposal()
        {
            LockProposal(ProjectAndConfigurationModel.SelectedProposal, this._lockProposalToggleButtonCommand.IsToggleChecked ?? false);
        }


        private void LockProposal(Proposal proposal, bool lockingMode)
        {
            Guid? lockedByUserId;
            var errorInfo = lockingMode ? ProjectAndConfigurationModel.SetLockOnProposal(proposal, LockingMode.Lock, out lockedByUserId) : ProjectAndConfigurationModel.SetLockOnProposal(proposal, LockingMode.Unlock, out lockedByUserId);
            if (errorInfo != null) throw new Exception(errorInfo.Message);
        }


        private bool CanExecuteToggleLockProposal(out string errorMessage)
        {
            errorMessage = string.Empty;
            return true;
        }


        private void ExecuteUpdateProposalList()
        {
            // Test mit Belegerstellung
            var creationMode = NewProposalCreationMode.NewProposalWithoutReference;
            Proposal sourceProposal = null; // not needed (only when creating an appendx or a copy from template)
            
            
            var destinationProject = ProjectAndConfigurationModel.GetProject(ProjectAndConfigurationModel.SelectedProjectInfo.InternalProjectID);
//            var destinationProject = ProjectAndConfigurationModel.GetProject(new Guid("03A4C3B8-0DBD-EA11-B80E-0050568F5578"));

            // Als Kopie
            creationMode = NewProposalCreationMode.NewProposalFromTemplate;
            //sourceProposal = ProjectAndConfigurationModel.GetProposal(new Guid("22055893-8174-EC11-9FBF-00D861D0BDD9"));
            //sourceProposal = ProjectAndConfigurationModel.GetFavoriteProposals(User.CurrentUser.LBUser).FirstOrDefault();
            sourceProposal = ProjectAndConfigurationModel.SelectedProposal;
            
            var createNewProposalIdArgs = new CreateNewProposalIdArgs { CreationMode = creationMode, SourceProposal = sourceProposal, DestinationProject = destinationProject };
            var newProposalId = ProjectAndConfigurationModel.GenerateNewProposalId(creationMode, destinationProject.InternalProjectID, sourceProposal?.InternalProposalID ?? Guid.Empty, out var proposalIdMainPart, out var proposalIdAppendixPart, out var editableDefinition);
            var newProposal = ProjectAndConfigurationModel.CreateNewProposal(createNewProposalIdArgs,
                newProposalId,
                proposalIdMainPart,
                proposalIdAppendixPart,
                User.CurrentUser.LBUser.UserID,
                new ClientDetails(true),
                out var errorMessageInfo);
            
            if (!string.IsNullOrEmpty(errorMessageInfo?.MessageText))
            {
                MessageBox.Show(errorMessageInfo.MessageText);
                return;
            }

            if (creationMode == NewProposalCreationMode.NewProposalWithoutReference)
            {
                // Create initial Configuration
                var mainConfigurations = ProjectAndConfigurationModel.LoadConstructionKitHeaders(User.CurrentUser.LBUser.Language, MasterStructureType.MainConfiguration);
                var usedMainConfiguration = mainConfigurations?.First(); // hier in geeigneter Weise einen Eintrag ermitteln
                ProjectAndConfigurationModel.InitializeConfigurationFromConstructionKit(newProposal, usedMainConfiguration);
            }



            //ProjectAndConfigurationModel.GetProposals(ProjectAndConfigurationModel?.SelectedProjectInfo?.InternalProjectID);
            //var internalProjectID = ProjectsAndProposalsViewModel.LoadAllProposals ? null : ProjectAndConfigurationModel?.SelectedProjectInfo?.InternalProjectID;
            //ProjectAndConfigurationModel.GetProposals(internalProjectID);
            
            //ProjectsAndProposalsViewModel.UpdateProposalList();

            ProjectsAndProposalsViewModel.ProposalGridSelectedItem = null;
            ProjectsAndProposalsViewModel.UpdateProposalList(EAS.LeegooBuilder.Common.CommonTypes.ProposalHelper.ProposalGridUpdateTypeEnumsClass.ProposalGridUpdateType.Insert);

        }
        
        
        private bool CanExecuteUpdateProposalList(out string errorMessage)
        {
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


        private void ExecuteReadPricingField()
        {
            var pricingfieldValue = SelectedConfigurationTreeItem?.Value?.F01?.Expression;
            MessageBox.Show(pricingfieldValue);
        }

        
        private bool CanExecuteReadPricingField(out string errorMessage)  => CheckIfConfigurationTreeItemIsSelected(out errorMessage);

            
        private void ExecuteWritePricingField()
        {
            var pricingField = SelectedConfigurationTreeItem?.Value?.F01;
            var roundExpression = pricingField.HasRoundExpression ? $" rnd {pricingField.Round}" : string.Empty;
            var newValue = pricingField.Value + 1;
            var newPricingField = new PricingField(Invariant($"{pricingField.Statement} {newValue}{roundExpression}"));
            SelectedConfigurationTreeItem.Value.F01 = newPricingField;
        }

        
        private bool CanExecuteWritePricingField(out string errorMessage) => CheckIfConfigurationTreeItemIsSelected(out errorMessage);

        
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

            const string scriptName = "HelloWorld";
            var script = ProjectAndConfigurationModel.LoadScriptByName(scriptName);
            if (script == null)
                MessageBox.Show("Scipt 'HelloWorld' not found!");

            else
            {
                var args = new CustomScriptArgs(ProjectAndConfigurationModel.SelectedProposal)
                {
                    CurrentConfigurationItemId = SelectedConfigurationTreeItem?.Value.ComponentID ?? Guid.Empty, CustomArgs = new object[] {"abc", 123}
                };

                // pass some custom parameters to the script
                
                MessageBox.Show($"Going to start Script '{script.Name}'");

                var scriptResult = ProjectAndConfigurationModel.ExecuteCustomScript(script.ScriptsKpId, args);

                var resultMessage = $"Script '{script.Name}' was executed.";
                if (scriptResult != null)
                    resultMessage = $"{resultMessage}\nResult is {scriptResult.ToString()}";
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


        private void ExecuteLoadGlobalAttributes()
        {
            ProjectAndConfigurationModel.LoadGlobalAttributes(SelectedConfigurationTreeItem.Value);
            MessageBox.Show($"{SelectedConfigurationTreeItem.Value.GlobalAttributes.Count} globale Merkmale wurden geladen.");
        }


        private bool CanExecuteLoadGlobalAttributes(out string errorMessage)
        {
            if (!CheckIfConfigurationTreeItemIsSelected(out errorMessage))
            {
                return false;
            }

            if (!SelectedConfigurationTreeItem.Value.HasConfigurator)
            {
                errorMessage = "No configurator selected!";
                return false;
            }

            errorMessage = string.Empty;
            return true;
        }


        private void ExecuteGetProposalCustomDefinitionValues()
        {
            var proposal = ProjectAndConfigurationModel.SelectedProposal; // nur der Übersicht halber (s.u.)
            var sampleCustomFieldName = "OMCEUNETGROSSFACTOR";



            // load the CustomDefinitions
            // Only one call is neccessary. This is just the definition. Values are stored in the  proposal instance.
            var customDefinitions = ProjectAndConfigurationModel.GetCustomDefinitionsInfos(CustomDefinitionTableType.Proposal);


            var sampleCustomDefinition = customDefinitions.FirstOrDefault(item => item.CustomFieldName.Equals(sampleCustomFieldName, StringComparison.InvariantCultureIgnoreCase));
            if (sampleCustomDefinition == null)
                throw new KeyNotFoundException($"Could not find {sampleCustomFieldName}");


            var customDefinitionValue = proposal.ProposalCustomDefinitionValues.FirstOrDefault(item => item.ProposalCustomDefinitionID == sampleCustomDefinition.ID);
            var value = customDefinitionValue == null ? string.Empty : customDefinitionValue.StringValue;


            MessageBox.Show($"Value of Custom Definition Values {sampleCustomFieldName}: {value}");

        }

        private bool CanExecuteGetProposalCustomDefinitionValues(out string errorMessage)
        {
            errorMessage = string.Empty;
            return true;
        }


        private void ExecuteSetLocalAttributes()
        {
            var easPriceList = SelectedConfigurationTreeItem.Parent.Value; //test
            var easPriceListElement = SelectedConfigurationTreeItem.Value.Element; //test


            var orderedItem = easPriceList.Children.FirstOrDefault(x => x.InternalElementID.HasValue && x.InternalElementID.Value == easPriceListElement.InternalElementID);

            // Check if there is an ordered item present
            if (orderedItem == null)
                MessageBox.Show("null!");


            var localAttributeInfos = ProjectAndConfigurationModel.GetLocalAttributes(SelectedConfigurationTreeItem.Value.ComponentID, SelectedConfigurationTreeItem.Value.Element.InternalElementID, User.CurrentUser.LBUser.Language);


            var localAttribute = localAttributeInfos.First();//OrDefault(x => x.AttributeName == quantityId);
            localAttribute.DataValue = "test2";
            ProjectAndConfigurationModel.SetLocalAttribute(SelectedConfigurationTreeItem.Value.ComponentID, localAttribute);
        }


        private bool CanExecuteSetLocalAttributes(out string errorMessage) => CheckIfConfigurationTreeItemIsSelected(out errorMessage);

        #region Helpers


        private void BeginUpdate() => ProjectAndConfigurationModel.BeginUpdateConfiguration();
        private void EndUpdate() 
        {
            ProjectAndConfigurationModel.EndUpdateConfiguration();
            RefreshConfigurationTree();
        }
        
        private bool CheckIfConfigurationTreeItemIsSelected(out string errorMessage)
        {
            if (SelectedConfigurationTreeItem == null)
            {
                errorMessage = "No configurationitem selected!";
                return false;
            }

            errorMessage = string.Empty;
            return true;
        }

        //public event EventHandler RefreshConfigurationTreeEvent;
        private void RefreshConfigurationTree()
        {
            //RefreshConfigurationTreeEvent?.Invoke(this, EventArgs.Empty);
            eventAggregator.GetEvent<ExecuteConfigurationTreeSmartUpdateEvent>().Publish(null);
            //ExecuteConfigurationTreeSmartUpdateEvent
        }
        
        #endregion Helpers 

        
        #region IO-Commands
        
        private void ExecuteSaveConfigurationItem()
        {
            ProjectAndConfigurationModel.SaveDynamicData();
        }
        
        private bool CanExecuteSaveConfigurationItem(out string errorMessage) => ClientValidator.ValidateCanExecuteSaveProposalChanges(ProjectAndConfigurationModel.SelectedProposal, ProjectAndConfigurationModel.IsRunningInTravelMode, out errorMessage);
            
        #endregion IO-Commands
        
        #region ConfigurationItem-Commands
        
        
        #region InsertElement-Command

        private void ExecuteInsertElement()
        {
            // akst user for element id
            var elementId = InputBox.Query("Element Id");
            var element = ProjectAndConfigurationModel.LoadByElementID(elementId);
            if (element == null)
            {
                MessageBox.Show($"Element with id '{elementId}' not found.");
                return;
            }

            
            var internalElementId = element.InternalElementID;


            BeginUpdate();
            
            // Insert the ConfigurationItem
            var parameters = new CreateConfigurationItemParameters(
                internalElementId,
                SelectedConfigurationTreeItem.Value.ComponentID,
                TreeStructureItemInsertMode.AddFirstChild);

            var newTreeItem = ProjectAndConfigurationModel.CreateConfigurationItemFromElement(parameters, out var errorMessage);

            EndUpdate();
        }

        
        private bool CanExecuteInsertElement(out string errorMessage) => CheckIfConfigurationTreeItemIsSelected(out errorMessage); 
        
        #endregion InsertElement-Command

        
        #region ExecuteUpdateConfigurationItem-Command

        private void ExecuteUpdateConfigurationItem()
        {
            SelectedConfigurationTreeItem.Value.Quantity++; 
        }
        private bool CanExecuteUpdateConfigurationItem(out string errorMessage) => CheckIfConfigurationTreeItemIsSelected(out errorMessage);

        #endregion ExecuteUpdateConfigurationItem-Command
        

        #region ExecuteDeleteConfigurationItem-Command

        private void ExecuteDeleteConfigurationItem()
        {
            if (serviceLocator.GetInstance<IMessageBox>().Show("Are you sure?", // Sollen die Unterkomponenten von '%1' erhalten bleiben?
                    Translator.Translate("Confirmation"),
                    MessageBoxButton.YesNo, MessageBoxImage.Question,
                    MessageBoxResult.No) == MessageBoxResult.Yes)
            {
                BeginUpdate();
                ProjectAndConfigurationModel.DeleteConfigurationItem(SelectedConfigurationTreeItem.Value.ComponentID);
                EndUpdate();
            }
        }
        
        private bool CanExecuteDeleteConfigurationItem(out string errorMessage) => CheckIfConfigurationTreeItemIsSelected(out errorMessage);

        #endregion ExecuteDeleteConfigurationItem-Command
        

        #region ExecuteMoveConfigurationItem-Command

        private void ExecuteMoveConfigurationItem(MoveDirection moveDirection)
        {
            var parentId = SelectedConfigurationTreeItem.Parent.Value.ComponentID;
            var errorMessage = string.Empty;
            
            switch (moveDirection)
            {
                case MoveDirection.Down:
                    ProjectAndConfigurationModel.MoveConfigurationItem(SelectedConfigurationTreeItem, parentId, TreeStructureItemInsertMode.AddChild, null, out errorMessage);
                    break;

                case MoveDirection.Up:
                    ProjectAndConfigurationModel.MoveConfigurationItem(SelectedConfigurationTreeItem, parentId, TreeStructureItemInsertMode.AddFirstChild, null, out errorMessage);
                    break;
                
                default:
                    throw new ArgumentException($"MoveDirection '{moveDirection}' not known.");
            }
            
        }
        private bool CanExecuteMoveConfigurationItem(out string errorMessage) => CheckIfConfigurationTreeItemIsSelected(out errorMessage);

        #endregion ExecuteMoveConfigurationItem-Command


        #region ExecuteMoveConfigurationItem-Command

        private void ExecuteCloneConfigurationItem()
        {
            var parameters = new InsertConfigurationItemParameters(SelectedConfigurationTreeItem.Branch(), SelectedConfigurationTreeItem.Value.ComponentID, TreeStructureItemInsertMode.After, ConfigurationItemOrigins.Manually);
            ProjectAndConfigurationModel.InsertConfigurationItem(parameters, out var errorMessage);
        }
        
        private bool CanExecuteCloneConfigurationItem(out string errorMessage) => CheckIfConfigurationTreeItemIsSelected(out errorMessage);

        #endregion ExecuteMoveConfigurationItem-Command

        
        #endregion ConfigurationItem-Commands
        
        
        #region Documents-Commands

        #region ExecuteGetDocumentAsPdf-Command

        private void ExecuteGetDocumentAsPdf()
        {
            // getl all proposal documents
            var proposalId = ProjectAndConfigurationModel.SelectedProposal?.InternalProposalID;
            var language = User.CurrentUser.LBUser.Language;
            var proposalDocuments = ProjectAndConfigurationModel.GetProposalDocumentsInfo(proposalId.Value, language, false);
            
            // test case: get the first document to be exported
            var proposalDocument = proposalDocuments.First();
            
            // create dialog
            var dialog = new SaveFileDialog();
            dialog.Title = "Save document as PDF";
            dialog.DefaultExt = ".pdf";
            dialog.FileName = Path.GetInvalidFileNameChars().Aggregate($"{proposalDocument.Description}.pdf", (current, c) => current.Replace(c.ToString(), string.Empty));

            // open dialog
            var dialogResult = dialog.ShowDialog();
            if (dialogResult == true)
            {
                // convert to PDF
                var pdfData = ProjectAndConfigurationModel.GetProposalDocumentAsPDFByteArray(proposalDocument.ProposalTextID);

                // save PDF to file
                File.WriteAllBytes(dialog.FileName, pdfData);

                // shell execute saved file
                var ps = new ProcessStartInfo(dialog.FileName) { UseShellExecute = true, Verb = "open" };
                Task.Factory.ExecuteAndWaitNonBlocking(() => Process.Start(ps));
            }
        }

        private bool CanExecuteGetDocumentAsPdf(out string errorMessage)
        {
            errorMessage = string.Empty;
            return true;
        }

        #endregion ExecuteGetDocumentAsPdf-Command
        
            
        #endregion Documents-Commands

        private void ExecuteFakeLoadingBar()
        {
            DevexpressLoadingBar.Show("Fake Loading...");
            Thread.Sleep(1000);
            DevexpressLoadingBar.ChangeText("Fake update proposal...");
            Thread.Sleep(200);
            DevexpressLoadingBar.ChangeText("Fake saving changes...");
            Thread.Sleep(1500);
            DevexpressLoadingBar.ChangeText("Fake completing process...");
            DevexpressLoadingBar.Hide();
        }
        
        private void ExecuteGenericDialogsPopup()
        {
            var dialog2 = new FormDialog("Form Dialog","This is an example of the new and improved Form Dialog");
            var parameter = new FormDialogParameter
            {
                DialogTitle = "Test Dialog",
                Message = "Test Text for Dialog",
            };
            
            var text = dialog2.AddTextbox("Textbox");
            var combo = dialog2.AddCombobox("Combobox", new List<object> { "123", "234" });
            
            var button = dialog2.AddButton("Button", "Open");
            button.ButtonClick += () => text.TextEdit.Text = "Button Clicked";
            
            if (dialog2.ShowDialog(System.Windows.MessageBoxButton.OK) == System.Windows.MessageBoxResult.OK)
            {
                MessageBox.Show("Popup closed");
            }
        }
    }
}
