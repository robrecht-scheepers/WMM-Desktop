using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using WMM.WPF.MVVM;

namespace WMM.WPF.Categories
{
    public class SelectDeleteCategoryFallbackViewModel : ObservableObject
    {
        private CategoryViewModel _selectedFallbackCategory;
        private RelayCommand _confirmCommand;
        private RelayCommand _cancelCommand;

        public SelectDeleteCategoryFallbackViewModel(IEnumerable<CategoryViewModel> categories, CategoryViewModel deletedCategory)
        {
            DeletedCategory = deletedCategory;
            Categories = categories.OrderBy(x => x.Name).ToList();
            Categories.Remove(deletedCategory);
            SelectedFallbackCategory = Categories.FirstOrDefault();
        }

        public List<CategoryViewModel> Categories { get; }

        public CategoryViewModel DeletedCategory { get; }

        public CategoryViewModel SelectedFallbackCategory
        {
            get => _selectedFallbackCategory;
            set => SetValue(ref _selectedFallbackCategory, value);
        }

        public bool Confirmed { get; set; }

        public RelayCommand ConfirmCommand => _confirmCommand ?? (_confirmCommand = new RelayCommand(Confirm));

        private void Confirm()
        {
            Confirmed = true;
        }

        public RelayCommand CancelCommand => _cancelCommand ?? (_cancelCommand = new RelayCommand(Cancel));

        private void Cancel()
        {
            SelectedFallbackCategory = null;
            Confirmed = false;
        }
    }
}
