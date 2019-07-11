﻿using Microsoft.Dynamics.AX.Metadata.MetaModel;
using Microsoft.Dynamics.Framework.Tools.Extensibility;
using Microsoft.Dynamics.Framework.Tools.MetaModel.Automation.BaseTypes;
using Microsoft.Dynamics.Framework.Tools.MetaModel.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSD365VSAddIn.ShowTheLabel
{
    /// <summary>
    /// Show the label for the element
    /// this will show the label / help label for the element
    /// If none found then look at the extended data type
    /// </summary>
    [Export(typeof(IDesignerMenu))]
    [DesignerMenuExportMetadata(AutomationNodeType = typeof(IEdtBase))]
    class ShowTheLabelMenuAddIn : DesignerMenuBase
    {
        #region Member variables
        private const string addinName = "SSD365VSAddIn.ShowTheLabelMenuAddIn";
        #endregion

        #region Properties
        /// <summary>
        /// Caption for the menu item. This is what users would see in the menu.
        /// </summary>
        public override string Caption
        {
            get
            {
                return AddinResources.ShowTheLabelMenuAddIn;
            }
        }

        /// <summary>
        /// Unique name of the add-in
        /// </summary>
        public override string Name
        {
            get
            {
                return addinName;
            }
        }
        #endregion

        #region Callbacks
        /// <summary>
        /// Called when user clicks on the add-in menu
        /// </summary>
        /// <param name="e">The context of the VS tools and metadata</param>
        public override void OnClick(AddinDesignerEventArgs e)
        {
            //Microsoft.Dynamics.AX.Metadata.Core.MetaModel.EntryPointType entryPointType;
            try
            {
                var selectedItem = e.SelectedElement as Microsoft.Dynamics.Framework.Tools.MetaModel.Automation.IRootElement;
                if (selectedItem != null)
                {
                    var metadataType = selectedItem.GetMetadataType();

                    if(selectedItem is IEdtBase)
                    {
                        var axEdt = Common.CommonUtil.GetModelSaveService().GetExtendedDataType(selectedItem.Name); 
                        var edtLabelInfo = this.getEdtBaseLabel(axEdt, new EdtLabelInfo());
                        edtLabelInfo.DecypherLabels();
                        System.Windows.Forms.MessageBox.Show("Label: " + edtLabelInfo.Label + Environment.NewLine + "Help: " + edtLabelInfo.HelpLabel);
                    }            
                    //LabelFactory labelFactory = LabelFactory.construct(selectedItem);
                    //labelFactory.ApplyLabel();
                }
            }
            catch (Exception ex)
            {
                CoreUtility.HandleExceptionWithErrorMessage(ex);
            }
        }

        #endregion

        protected EdtLabelInfo getEdtBaseLabel(AxEdt edtBase, EdtLabelInfo labelInfo)
        {
            if(String.IsNullOrEmpty(labelInfo.Label) == true 
                && String.IsNullOrEmpty(edtBase.Label) == false)
            {
                // find the label here
                labelInfo.Label = edtBase.Label;
            }
            if(String.IsNullOrEmpty(labelInfo.HelpLabel) == true 
                && String.IsNullOrEmpty(edtBase.HelpText) == false)
            {
                // find the help label here
                labelInfo.HelpLabel = edtBase.HelpText;
            }

            if(labelInfo.RequiresDigging() == true
                && String.IsNullOrEmpty(edtBase.Extends) == false)
            {
                // if there is a extended data type to this then move it to the next one

                var edtExt = Common.CommonUtil.GetModelSaveService().GetExtendedDataType(edtBase.Extends);

                //IEdtBase extendedEdt = null;
                //foreach (IEdtBase edtExtended in edtBase.ExtendsValues)
                //{
                //    if (edtExtended.Name.Equals(edtBase.Name, StringComparison.InvariantCultureIgnoreCase))
                //    {
                //        extendedEdt = edtExtended;
                //        break;
                //    }
                //}

                if (edtExt != null)
                {
                    labelInfo = this.getEdtBaseLabel(edtExt, labelInfo);
                }
            }

            return labelInfo;
        }



        public class EdtLabelInfo
        {
            public string Label { get; set; }
            public string HelpLabel { get; set; }

            public bool RequiresDigging()
            {
                if(String.IsNullOrEmpty(this.Label)
                    || String.IsNullOrEmpty(HelpLabel))
                {
                    return true;
                }
                return false;
            }

            public void DecypherLabels()
            {
                if(this.Label.StartsWith("@"))
                {
                    var labelContent = Labels.LabelHelper.FindLabelGlobally(this.Label);
                    if (String.IsNullOrEmpty(labelContent.LabelText) == false)
                    {
                        this.Label = labelContent.LabelText;
                    }
                    
                }

                if (this.HelpLabel.StartsWith("@"))
                {
                    var labelContent = Labels.LabelHelper.FindLabelGlobally(this.HelpLabel);
                    if (String.IsNullOrEmpty(labelContent.LabelText) == false)
                    {
                        this.HelpLabel = labelContent.LabelText;
                    }

                }
            }
        }

    }


}
