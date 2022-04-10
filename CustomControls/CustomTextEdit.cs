using DevExpress.Utils;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraEditors.Drawing;
using DevExpress.XtraEditors.Registrator;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraEditors.ViewInfo;
using System;
using System.ComponentModel;
using System.Drawing;

namespace Sensoy_IpTv
{
    [UserRepositoryItem("RegisterCustomTextEdit")]
    public class RepositoryItemCustomTextEdit : RepositoryItemButtonEdit
    {
        public const string CustomEditName = "CustomTextEdit";
        public override string EditorTypeName => CustomEditName;

        static RepositoryItemCustomTextEdit()
        {
            RegisterCustomTextEdit();
        }

        public RepositoryItemCustomTextEdit()
        {
        }

        public static void RegisterCustomTextEdit()
        {
            Image img = null;
            EditorRegistrationInfo.Default.Editors.Add(new EditorClassInfo(CustomEditName, typeof(CustomTextEdit), typeof(RepositoryItemCustomTextEdit), typeof(MyButtonEditViewInfo), new MyButtonEditPainter(), true, img));
        }

        public override void CreateDefaultButton()
        {
        }

        readonly EditorButton temizle = new EditorButton(ButtonPredefines.Glyph, "", -1, true, false, false, new EditorButtonImageOptions() { SvgImageSize = new Size(16, 16), SvgImage = Sensoy_IpTv.Properties.Resources.close }, new KeyShortcut(System.Windows.Forms.Keys.None), "Temizle");
        readonly EditorButton sifre = new EditorButton(ButtonPredefines.Glyph, "", -1, true, false, false, new EditorButtonImageOptions() { SvgImageSize = new Size(16, 16), SvgImage = Sensoy_IpTv.Properties.Resources.visibility_off }, new KeyShortcut(System.Windows.Forms.Keys.None), "Gizle");

        public override bool UseSystemPasswordChar { get => base.UseSystemPasswordChar; set { base.UseSystemPasswordChar = value; sifre.ToolTip = value ? "Göster" : "Gizle"; sifre.ImageOptions.SvgImage = value ? Sensoy_IpTv.Properties.Resources.visibility : Sensoy_IpTv.Properties.Resources.visibility_off; } }
        public bool SifreKutusu { get => sifrekutusu; set => sifrekutusu = value; }
        private bool aktif = false, sifrekutusu = false;

        public override void EndInit()
        {
            base.EndInit();
            Buttons.Clear();
            if (!IsDesignMode)
            {
                Buttons.AddRange(new EditorButton[] { temizle, sifre });
                MouseEnter += RepositoryItemCustomTextEdit_MouseEnter;
                MouseLeave += RepositoryItemCustomTextEdit_MouseLeave;
                ButtonClick += RepositoryItemCustomTextEdit_ButtonClick;
                EditValueChanged += RepositoryItemCustomTextEdit_EditValueChanged;
            }
        }

        private void RepositoryItemCustomTextEdit_MouseLeave(object sender, EventArgs e)
        {
            aktif = false;
            temizle.Visible = false;
            sifre.Visible = false;
        }

        private void RepositoryItemCustomTextEdit_MouseEnter(object sender, EventArgs e)
        {
            aktif = true;
            temizle.Visible = !string.IsNullOrEmpty(OwnerEdit.Text);
            sifre.Visible = SifreKutusu;
        }

        private void RepositoryItemCustomTextEdit_EditValueChanged(object sender, EventArgs e)
        {
            if (aktif)
                temizle.Visible = !string.IsNullOrEmpty(OwnerEdit.Text);
        }

        private void RepositoryItemCustomTextEdit_ButtonClick(object sender, ButtonPressedEventArgs e)
        {
            if (e.Button == temizle)
                OwnerEdit.Text = "";
            else if (e.Button == sifre)
                UseSystemPasswordChar = !UseSystemPasswordChar;
        }
    }

    [ToolboxItem(true)]
    public class CustomTextEdit : ButtonEdit
    {
        static CustomTextEdit()
        {
            RepositoryItemCustomTextEdit.RegisterCustomTextEdit();
        }

        public CustomTextEdit()
        {
            Properties.UseAdvancedMode = DefaultBoolean.True;
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public new RepositoryItemCustomTextEdit Properties => base.Properties as RepositoryItemCustomTextEdit;
        public override string EditorTypeName => RepositoryItemCustomTextEdit.CustomEditName;
    }

    public class MyButtonEditViewInfo : ButtonEditViewInfo
    {
        public MyButtonEditViewInfo(RepositoryItem item) : base(item)
        {
        }
    }

    public class MyButtonEditPainter : ButtonEditPainter
    {
        public MyButtonEditPainter()
        {
        }
    }
}