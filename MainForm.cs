namespace TinyPad;

using ScintillaNET;

public partial class MainForm : Form
{
    private string _currentFilePath = string.Empty;
    private bool _isDirty = false;
    private Scintilla _textEditor = null!;
    private StatusStrip _statusBar = null!;
    private ToolStripStatusLabel _statusLabel = null!;
    private ToolStripMenuItem _wordWrapMenuItem = null!;
    private Form _findDialog = null!;
    private TextBox _findTextBox = null!;
    private TextBox _replaceTextBox = null!;

    public MainForm()
    {
        InitializeComponent();
        Text = "tiny - untitled";
        Icon = SystemIcons.Application;
        ApplyDarkTheme();
    }

    public MainForm(string? filePath)
    {
        InitializeComponent();
        Text = "tiny - untitled";
        Icon = SystemIcons.Application;
        ApplyDarkTheme();

        if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
        {
            try
            {
                _textEditor.Text = File.ReadAllText(filePath);
                _currentFilePath = filePath;
                _isDirty = false;
                Text = $"tiny - {Path.GetFileName(_currentFilePath)}";
            }
            catch { }
        }
    }

    private void ApplyDarkTheme()
    {
        // VS Code dark theme colors
        BackColor = Color.FromArgb(30, 30, 30);      // #1e1e1e
        ForeColor = Color.FromArgb(212, 212, 212);   // #d4d4d4
        FormBorderStyle = FormBorderStyle.None;
        _statusBar.BackColor = Color.FromArgb(30, 30, 30);    // #1e1e1e (matching editor)
        _statusBar.ForeColor = Color.FromArgb(133, 133, 133); // #858585
    }

    private void InitializeComponent()
    {
        var tableLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = 3,
            ColumnCount = 1,
            RowStyles =
            {
                new RowStyle(SizeType.AutoSize),
                new RowStyle(SizeType.Percent, 100),
                new RowStyle(SizeType.AutoSize)
            }
        };

        var menuStrip = CreateMenuStrip();
        tableLayout.Controls.Add(menuStrip, 0, 0);

        _textEditor = new Scintilla { Dock = DockStyle.Fill };
        ConfigureScintilla();
        _textEditor.TextChanged += TextEditor_TextChanged;
        _textEditor.UpdateUI += (s, e) => UpdateStatusBar();
        tableLayout.Controls.Add(_textEditor, 0, 1);

        _statusBar = new StatusStrip();
        _statusLabel = new ToolStripStatusLabel("Line 1, Col 1");
        _statusBar.Items.Add(_statusLabel);
        tableLayout.Controls.Add(_statusBar, 0, 2);

        _findDialog = CreateFindDialog();

        Controls.Add(tableLayout);
        ClientSize = new Size(800, 600);
        StartPosition = FormStartPosition.CenterScreen;
    }

    private void ConfigureScintilla()
    {
        _textEditor.Lexer = Lexer.Cpp;

        // VS Code dark theme colors
        var darkBg = Color.FromArgb(30, 30, 30);         // #1e1e1e
        var lightFg = Color.FromArgb(212, 212, 212);     // #d4d4d4
        var keywordColor = Color.FromArgb(86, 156, 214); // #569cd6
        var stringColor = Color.FromArgb(206, 145, 120); // #ce9178
        var commentColor = Color.FromArgb(106, 153, 85); // #6a9955

        // Reset and apply default style
        _textEditor.StyleResetDefault();
        _textEditor.Styles[Style.Default].BackColor = darkBg;
        _textEditor.Styles[Style.Default].ForeColor = lightFg;
        _textEditor.Styles[Style.Default].Font = "Consolas";
        _textEditor.Styles[Style.Default].Size = 10;

        // Clear all styles to inherit from default
        _textEditor.StyleClearAll();

        // Apply dark background to all style indices (0-127)
        for (int i = 0; i < 128; i++)
        {
            _textEditor.Styles[i].BackColor = darkBg;
            if (_textEditor.Styles[i].ForeColor == Color.Black)
                _textEditor.Styles[i].ForeColor = lightFg;
        }

        // Keywords (C++ style 5)
        _textEditor.Styles[5].ForeColor = keywordColor;
        _textEditor.Styles[5].Bold = true;

        // Strings (style 6, 7)
        _textEditor.Styles[6].ForeColor = stringColor;
        _textEditor.Styles[6].BackColor = darkBg;
        _textEditor.Styles[7].ForeColor = stringColor;
        _textEditor.Styles[7].BackColor = darkBg;

        // Comments (style 1, 2)
        _textEditor.Styles[1].ForeColor = commentColor;
        _textEditor.Styles[1].BackColor = darkBg;
        _textEditor.Styles[2].ForeColor = commentColor;
        _textEditor.Styles[2].BackColor = darkBg;

        _textEditor.WrapMode = WrapMode.None;
        _textEditor.UseTabs = false;
        _textEditor.TabWidth = 4;

        // Line numbers styling
        _textEditor.Margins[0].Width = 35;
        _textEditor.Margins[0].BackColor = Color.FromArgb(30, 30, 30);    // #1e1e1e
        _textEditor.Styles[Style.LineNumber].BackColor = Color.FromArgb(30, 30, 30);
        _textEditor.Styles[Style.LineNumber].ForeColor = Color.FromArgb(133, 133, 133); // #858585

        // Caret
        _textEditor.CaretForeColor = lightFg;
        _textEditor.CaretLineBackColor = Color.FromArgb(45, 45, 45); // Subtle highlight

        // Selection colors
        _textEditor.SetSelectionForeColor(true, lightFg);
        _textEditor.SetSelectionBackColor(true, Color.FromArgb(61, 116, 165)); // VS Code selection

        _textEditor.SetKeywords(0, "abstract as base bool break byte case catch char checked class const continue decimal default delegate do double else enum event explicit extern false finally fixed float for foreach goto if implicit in interface internal is lock long namespace new null object operator out override params private protected public readonly ref return sbyte sealed short sizeof stackalloc static string struct switch this throw true try typeof uint ulong unchecked unsafe ushort using var virtual void volatile while");
    }
    private MenuStrip CreateMenuStrip()
    {
        var menuStrip = new MenuStrip();

        var fileMenu = new ToolStripMenuItem("&File");
        fileMenu.DropDownItems.Add(CreateMenuItem("&New", Keys.Control | Keys.N, File_New));
        fileMenu.DropDownItems.Add(CreateMenuItem("&Open", Keys.Control | Keys.O, File_Open));
        fileMenu.DropDownItems.Add(CreateMenuItem("&Save", Keys.Control | Keys.S, File_Save));
        fileMenu.DropDownItems.Add(CreateMenuItem("Save &As", Keys.Control | Keys.Shift | Keys.S, File_SaveAs));
        fileMenu.DropDownItems.Add(new ToolStripSeparator());
        fileMenu.DropDownItems.Add(CreateMenuItem("E&xit", Keys.Alt | Keys.F4, File_Exit));
        menuStrip.Items.Add(fileMenu);

        var editMenu = new ToolStripMenuItem("&Edit");
        editMenu.DropDownItems.Add(CreateMenuItem("&Undo", Keys.Control | Keys.Z, Edit_Undo));
        editMenu.DropDownItems.Add(CreateMenuItem("&Redo", Keys.Control | Keys.Y, Edit_Redo));
        editMenu.DropDownItems.Add(new ToolStripSeparator());
        editMenu.DropDownItems.Add(CreateMenuItem("Cu&t", Keys.Control | Keys.X, Edit_Cut));
        editMenu.DropDownItems.Add(CreateMenuItem("&Copy", Keys.Control | Keys.C, Edit_Copy));
        editMenu.DropDownItems.Add(CreateMenuItem("&Paste", Keys.Control | Keys.V, Edit_Paste));
        editMenu.DropDownItems.Add(new ToolStripSeparator());
        editMenu.DropDownItems.Add(CreateMenuItem("&Find", Keys.Control | Keys.F, Edit_Find));
        editMenu.DropDownItems.Add(CreateMenuItem("&Replace", Keys.Control | Keys.H, Edit_Replace));
        editMenu.DropDownItems.Add(new ToolStripSeparator());
        editMenu.DropDownItems.Add(CreateMenuItem("Select &All", Keys.Control | Keys.A, Edit_SelectAll));
        menuStrip.Items.Add(editMenu);

        var viewMenu = new ToolStripMenuItem("&View");
        _wordWrapMenuItem = new ToolStripMenuItem("&Word Wrap", null, View_WordWrap) { Checked = false };
        viewMenu.DropDownItems.Add(_wordWrapMenuItem);
        viewMenu.DropDownItems.Add(new ToolStripSeparator());
        viewMenu.DropDownItems.Add(CreateMenuItem("&Fullscreen", Keys.F11, View_Fullscreen));
        menuStrip.Items.Add(viewMenu);

        menuStrip.BackColor = Color.FromArgb(51, 51, 51);      // #333333
        menuStrip.ForeColor = Color.FromArgb(212, 212, 212);   // #d4d4d4

        return menuStrip;
    }

    private ToolStripMenuItem CreateMenuItem(string text, Keys shortcut, EventHandler handler)
    {
        return new ToolStripMenuItem(text, null, handler) { ShortcutKeys = shortcut };
    }

    private Form CreateFindDialog()
    {
        var form = new Form
        {
            Text = "Find & Replace",
            Size = new Size(400, 150),
            StartPosition = FormStartPosition.CenterParent,
            ShowInTaskbar = false,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            MaximizeBox = false,
            MinimizeBox = false,
            BackColor = Color.FromArgb(51, 51, 51),      // #333333
            ForeColor = Color.FromArgb(212, 212, 212)    // #d4d4d4
        };

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = 3,
            ColumnCount = 2,
            Padding = new Padding(10),
            BackColor = Color.FromArgb(51, 51, 51)
        };

        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70));

        var findLabel = new Label { Text = "Find:", AutoSize = true, ForeColor = Color.FromArgb(212, 212, 212) };
        layout.Controls.Add(findLabel, 0, 0);
        _findTextBox = new TextBox { Dock = DockStyle.Fill, BackColor = Color.FromArgb(60, 60, 60), ForeColor = Color.FromArgb(212, 212, 212) };
        layout.Controls.Add(_findTextBox, 1, 0);

        var replaceLabel = new Label { Text = "Replace:", AutoSize = true, ForeColor = Color.FromArgb(212, 212, 212) };
        layout.Controls.Add(replaceLabel, 0, 1);
        _replaceTextBox = new TextBox { Dock = DockStyle.Fill, BackColor = Color.FromArgb(60, 60, 60), ForeColor = Color.FromArgb(212, 212, 212) };
        layout.Controls.Add(_replaceTextBox, 1, 1);

        var buttonPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            AutoSize = true,
            BackColor = Color.FromArgb(51, 51, 51)
        };

        var findNextBtn = new Button { Text = "Find Next", Width = 80, BackColor = Color.FromArgb(60, 60, 60), ForeColor = Color.FromArgb(212, 212, 212) };
        findNextBtn.Click += (s, e) => FindNext();
        buttonPanel.Controls.Add(findNextBtn);

        var replaceBtn = new Button { Text = "Replace", Width = 80, BackColor = Color.FromArgb(60, 60, 60), ForeColor = Color.FromArgb(212, 212, 212) };
        replaceBtn.Click += (s, e) => ReplaceOne();
        buttonPanel.Controls.Add(replaceBtn);

        var replaceAllBtn = new Button { Text = "Replace All", Width = 80, BackColor = Color.FromArgb(60, 60, 60), ForeColor = Color.FromArgb(212, 212, 212) };
        replaceAllBtn.Click += (s, e) => ReplaceAll();
        buttonPanel.Controls.Add(replaceAllBtn);

        var closeBtn = new Button { Text = "Close", Width = 80, BackColor = Color.FromArgb(60, 60, 60), ForeColor = Color.FromArgb(212, 212, 212) };
        closeBtn.Click += (s, e) => form.Close();
        buttonPanel.Controls.Add(closeBtn);

        layout.Controls.Add(buttonPanel, 0, 2);
        layout.SetColumnSpan(buttonPanel, 2);

        form.Controls.Add(layout);
        return form;
    }

    private void File_New(object? sender, EventArgs e)
    {
        if (_isDirty)
        {
            var result = MessageBox.Show("Save changes?", "Unsaved Changes", MessageBoxButtons.YesNoCancel);
            if (result == DialogResult.Cancel) return;
            if (result == DialogResult.Yes) File_Save(null, EventArgs.Empty);
        }

        _textEditor.Text = "";
        _currentFilePath = string.Empty;
        _isDirty = false;
        Text = "tiny - untitled";
    }

    private void File_Open(object? sender, EventArgs e)
    {
        if (_isDirty)
        {
            var result = MessageBox.Show("Save changes?", "Unsaved Changes", MessageBoxButtons.YesNoCancel);
            if (result == DialogResult.Cancel) return;
            if (result == DialogResult.Yes) File_Save(null, EventArgs.Empty);
        }

        using (var dialog = new OpenFileDialog { Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*" })
        {
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    _textEditor.Text = File.ReadAllText(dialog.FileName);
                    _currentFilePath = dialog.FileName;
                    _isDirty = false;
                    Text = $"tiny - {Path.GetFileName(_currentFilePath)}";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error opening file: {ex.Message}", "Error");
                }
            }
        }
    }

    private void File_Save(object? sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(_currentFilePath))
        {
            File_SaveAs(null, EventArgs.Empty);
            return;
        }

        try
        {
            File.WriteAllText(_currentFilePath, _textEditor.Text);
            _isDirty = false;
            Text = $"tiny - {Path.GetFileName(_currentFilePath)}";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error saving file: {ex.Message}", "Error");
        }
    }

    private void File_SaveAs(object? sender, EventArgs e)
    {
        using (var dialog = new SaveFileDialog { Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*" })
        {
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    File.WriteAllText(dialog.FileName, _textEditor.Text);
                    _currentFilePath = dialog.FileName;
                    _isDirty = false;
                    Text = $"tiny - {Path.GetFileName(_currentFilePath)}";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error saving file: {ex.Message}", "Error");
                }
            }
        }
    }

    private void File_Exit(object? sender, EventArgs e) => Close();

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        if (_isDirty)
        {
            var result = MessageBox.Show("Save changes before closing?", "Unsaved Changes", MessageBoxButtons.YesNoCancel);
            if (result == DialogResult.Cancel)
                e.Cancel = true;
            else if (result == DialogResult.Yes)
                File_Save(null, EventArgs.Empty);
        }
        base.OnFormClosing(e);
    }

    private void Edit_Undo(object? sender, EventArgs e) => _textEditor.Undo();
    private void Edit_Redo(object? sender, EventArgs e) => _textEditor.Redo();
    private void Edit_Cut(object? sender, EventArgs e) => _textEditor.Cut();
    private void Edit_Copy(object? sender, EventArgs e) => _textEditor.Copy();
    private void Edit_Paste(object? sender, EventArgs e) => _textEditor.Paste();
    private void Edit_SelectAll(object? sender, EventArgs e) => _textEditor.SelectAll();

    private void Edit_Find(object? sender, EventArgs e)
    {
        _replaceTextBox.Visible = false;
        _findDialog.Text = "Find";
        _findDialog.ShowDialog(this);
    }

    private void Edit_Replace(object? sender, EventArgs e)
    {
        _replaceTextBox.Visible = true;
        _findDialog.Text = "Find & Replace";
        _findDialog.ShowDialog(this);
    }

    private void FindNext()
    {
        string searchText = _findTextBox.Text;
        if (string.IsNullOrEmpty(searchText)) return;

        int startPos = _textEditor.CurrentPosition;
        int foundIndex = _textEditor.Text.IndexOf(searchText, startPos, StringComparison.CurrentCultureIgnoreCase);

        if (foundIndex == -1)
            foundIndex = _textEditor.Text.IndexOf(searchText, 0, StringComparison.CurrentCultureIgnoreCase);

        if (foundIndex != -1)
        {
            _textEditor.SetSelection(foundIndex, foundIndex + searchText.Length);
            _textEditor.ScrollCaret();
        }
        else
        {
            MessageBox.Show("Text not found.", "Find");
        }
    }

    private void ReplaceOne()
    {
        string searchText = _findTextBox.Text;
        string replaceText = _replaceTextBox.Text;
        if (string.IsNullOrEmpty(searchText)) return;

        var selectedText = _textEditor.GetTextRange(_textEditor.SelectionStart, _textEditor.SelectionEnd);
        if (selectedText.Equals(searchText, StringComparison.CurrentCultureIgnoreCase))
        {
            _textEditor.ReplaceSelection(replaceText);
            _isDirty = true;
        }

        FindNext();
    }

    private void ReplaceAll()
    {
        string searchText = _findTextBox.Text;
        string replaceText = _replaceTextBox.Text;
        if (string.IsNullOrEmpty(searchText)) return;

        int count = 0;
        _textEditor.TargetStart = 0;
        _textEditor.TargetEnd = _textEditor.TextLength;
        _textEditor.SearchFlags = SearchFlags.None;

        while (_textEditor.SearchInTarget(searchText) != -1)
        {
            _textEditor.ReplaceTarget(replaceText);
            _textEditor.TargetStart = _textEditor.TargetEnd;
            _textEditor.TargetEnd = _textEditor.TextLength;
            count++;
        }

        if (count > 0)
            _isDirty = true;

        MessageBox.Show($"Replaced {count} occurrence(s).", "Replace All");
    }

    private void View_WordWrap(object? sender, EventArgs e)
    {
        _textEditor.WrapMode = _textEditor.WrapMode == WrapMode.None ? WrapMode.Word : WrapMode.None;
        _wordWrapMenuItem.Checked = _textEditor.WrapMode != WrapMode.None;
    }

    private void View_Fullscreen(object? sender, EventArgs e)
    {
        if (WindowState == FormWindowState.Maximized)
        {
            WindowState = FormWindowState.Normal;
            FormBorderStyle = FormBorderStyle.FixedSingle;
        }
        else
        {
            FormBorderStyle = FormBorderStyle.None;
            WindowState = FormWindowState.Maximized;
        }
    }

    private void TextEditor_TextChanged(object? sender, EventArgs e)
    {
        if (!_isDirty)
        {
            _isDirty = true;
            if (!string.IsNullOrEmpty(_currentFilePath))
                Text = $"tiny - {Path.GetFileName(_currentFilePath)}*";
            else
                Text = "tiny - untitled*";
        }
    }

    private void UpdateStatusBar()
    {
        int line = _textEditor.CurrentLine + 1;
        int col = _textEditor.CurrentPosition - _textEditor.Lines[_textEditor.CurrentLine].Position + 1;
        _statusLabel.Text = $"Line {line}, Col {col}";
    }
}
