//  ---------------------------------------------------------------------------------
//  Copyright (c) Microsoft Corporation.  All rights reserved.
// 
//  The MIT License (MIT)
// 
//  Permission is hereby granted, free of charge, to any person obtaining a copy
//  of this software and associated documentation files (the "Software"), to deal
//  in the Software without restriction, including without limitation the rights
//  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//  copies of the Software, and to permit persons to whom the Software is
//  furnished to do so, subject to the following conditions:
// 
//  The above copyright notice and this permission notice shall be included in
//  all copies or substantial portions of the Software.
// 
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//  THE SOFTWARE.
//  ---------------------------------------------------------------------------------

using ContosoModels;
using ContosoApp.Commands;
using PropertyChanged;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace ContosoApp.ViewModels
{
    [ImplementPropertyChanged]
    /// <summary>
    /// Encapsulates data for the CustomerListPage. The page UI
    /// binds to the properties defined here. 
    /// </summary>
    public class CustomerListPageViewModel : BindableBase
    {
        public CustomerListPageViewModel()
        {
            Task.Run(GetCustomerList);

            RefreshCommand = new RelayCommand(OnRefresh);
        }
         
        public ObservableCollection<CustomerViewModel2> Customers2 { get; set; } = 
            new ObservableCollection<CustomerViewModel2>(); 

        private CustomerViewModel2 _selectedCustomer2;

        public CustomerViewModel2 SelectedCustomer2
        {
            get { return _selectedCustomer2; }
            set
            {
                SetProperty(ref _selectedCustomer2, value); 
            }
        }

        private string _errorText = null;

        /// <summary>
        /// Gets or sets the error text.
        /// </summary>
        public string ErrorText
        {
            get { return _errorText; }

            set
            {
                SetProperty(ref _errorText, value);
            }
        }


        /// <summary>
        /// Gets or sets whether to show the data loading progress wheel. 
        /// </summary>
        public bool IsLoading { get; private set; } = false;

        /// <summary>
        /// Gets the complete list of customers from the database.
        /// </summary>
        private async Task GetCustomerList()
        {
            await Utilities.CallOnUiThreadAsync(() => IsLoading = true); 

            var db = new ContosoDataSource();
            var customers = await db.Customers.GetAsync();

            await Utilities.CallOnUiThreadAsync(() =>
            {
                foreach (var c in customers)
                {
                    Customers2.Add(new CustomerViewModel2(c)); 
                }
                IsLoading = false;
            });
        }

        public RelayCommand RefreshCommand { get; private set; }

        /// <summary>
        /// Queries the database for a current list of customers.
        /// </summary>
        private void OnRefresh()
        {
            Task.Run(async () =>
            {
                IsLoading = true; 
                var db = new ContosoDataSource(); 
                var modified = Customers2.Where(x => x._isModified).Select(x => x._model);
                foreach (var mc in modified)
                {
                    await db.Customers.PostAsync(mc); 
                }
                await GetCustomerList();
                IsLoading = false; 
            }); 
        }
    }
}
