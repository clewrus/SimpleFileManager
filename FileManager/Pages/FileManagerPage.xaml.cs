﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using SimpleFM.FileManager.ViewModels;
using SimpleFM.ViewModels;

namespace SimpleFM.FileManager.Pages {
	/// <summary>
	/// Логика взаимодействия для FileManagerPage.xaml
	/// </summary>
	public partial class FileManagerPage : Page {
		public FileManagerPage () {
			InitializeComponent();
			this.DataContext = new FileManagerViewModel();
		}
	}
}
