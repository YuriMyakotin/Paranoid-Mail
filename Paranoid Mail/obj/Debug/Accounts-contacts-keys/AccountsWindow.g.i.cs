﻿#pragma checksum "..\..\..\Accounts-contacts-keys\AccountsWindow.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "068CA703DD9A6B15DB7CC0AFC2C02130"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using Paranoid;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Controls.Ribbon;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;


namespace Paranoid {
    
    
    /// <summary>
    /// AccountsWindow
    /// </summary>
    public partial class AccountsWindow : System.Windows.Window, System.Windows.Markup.IComponentConnector {
        
        
        #line 14 "..\..\..\Accounts-contacts-keys\AccountsWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ListBox AccountsBox;
        
        #line default
        #line hidden
        
        
        #line 18 "..\..\..\Accounts-contacts-keys\AccountsWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button CancelButton;
        
        #line default
        #line hidden
        
        
        #line 27 "..\..\..\Accounts-contacts-keys\AccountsWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button ButtonNew;
        
        #line default
        #line hidden
        
        
        #line 34 "..\..\..\Accounts-contacts-keys\AccountsWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button ButtonChange;
        
        #line default
        #line hidden
        
        
        #line 42 "..\..\..\Accounts-contacts-keys\AccountsWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button ButtonDelete;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/Paranoid Mail;component/accounts-contacts-keys/accountswindow.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\Accounts-contacts-keys\AccountsWindow.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            this.AccountsBox = ((System.Windows.Controls.ListBox)(target));
            return;
            case 2:
            this.CancelButton = ((System.Windows.Controls.Button)(target));
            return;
            case 3:
            this.ButtonNew = ((System.Windows.Controls.Button)(target));
            
            #line 32 "..\..\..\Accounts-contacts-keys\AccountsWindow.xaml"
            this.ButtonNew.Click += new System.Windows.RoutedEventHandler(this.NewButton_OnClick);
            
            #line default
            #line hidden
            return;
            case 4:
            this.ButtonChange = ((System.Windows.Controls.Button)(target));
            
            #line 40 "..\..\..\Accounts-contacts-keys\AccountsWindow.xaml"
            this.ButtonChange.Click += new System.Windows.RoutedEventHandler(this.ChangeButton_OnClick);
            
            #line default
            #line hidden
            return;
            case 5:
            this.ButtonDelete = ((System.Windows.Controls.Button)(target));
            return;
            }
            this._contentLoaded = true;
        }
    }
}

