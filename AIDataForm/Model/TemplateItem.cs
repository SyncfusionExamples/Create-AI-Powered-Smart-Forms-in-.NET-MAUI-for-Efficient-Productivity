namespace AIDataForm
{
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using Syncfusion.Maui.AIAssistView;

    /// <summary>
    /// The template item.
    /// </summary>
    public class TemplateItem
    {
        /// <summary>
        /// Gets or sets the font icon.
        /// </summary>
        public string? Font { get; set; }

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        public string? Title { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        public string? Description { get; set; }
    }
}