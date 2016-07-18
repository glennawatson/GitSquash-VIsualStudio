namespace GitSquash.VisualStudio.Extension.View
{
    using System.Windows;

    using ViewModel;

    /// <summary>
    /// Interaction logic for SquashView.xaml
    /// </summary>
    public partial class SquashView 
    {
        /// <summary>
        /// Dependency property for the view model.
        /// </summary>
        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register("ViewModel", typeof(ISquashViewModel), typeof(SquashView), new PropertyMetadata(default(ISquashViewModel)));

        /// <summary>
        /// Initializes a new instance of the <see cref="SquashView"/> class.
        /// </summary>
        public SquashView()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Gets or sets the view model.
        /// </summary>
        public ISquashViewModel ViewModel
        {
            get
            {
                return (ISquashViewModel)this.GetValue(ViewModelProperty);
            }

            set
            {
                this.SetValue(ViewModelProperty, value);
            }
        }
    }
}
