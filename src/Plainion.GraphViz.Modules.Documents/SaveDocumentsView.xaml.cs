﻿using System.ComponentModel.Composition;
using System.Windows.Controls;

namespace Plainion.GraphViz.Modules.Documents
{
    [Export( typeof( SaveDocumentsView ) )]
    public partial class SaveDocumentsView : UserControl
    {
        [ImportingConstructor]
        public SaveDocumentsView(SaveDocumentsViewModel model)
        {
            InitializeComponent();

            DataContext = model;
        }
    }
}
