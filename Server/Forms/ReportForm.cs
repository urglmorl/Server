﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

using Server.Helpers;
using Server.Models;

using Excel = Microsoft.Office.Interop.Excel;

namespace Server.Forms
{
    public partial class ReportForm : Form
    {
        public ReportForm()
        {
            InitializeComponent();
        }

        private void ReportForm_Load(object sender, EventArgs e)
        {

        }

        private void ComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (reportTypeComboBox.SelectedIndex)
            {
                case 0: 
                    {
                        byGroupReportGroupBox.Visible = true;
                        byGroupReportGroupBox.Location = new Point(4, 76);

                        List<Group> groups = DatabaseHelper.GetGroups();

                        foreach (var item in groups)
                        {
                            groupsComboBox.Items.Add(item);
                        }

                        List<Subject> subjects = DatabaseHelper.GetSubjects();

                        foreach (var item in subjects)
                        {
                            subjectsComboBox.Items.Add(item);
                        }

                        this.Height = 400; 

                        break;
                    }
            }
        }

        private void SubjectsComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            Subject subject = subjectsComboBox.SelectedItem as Subject;
            List<Theme> themes = DatabaseHelper.GetThemes(subject.Id);
            themesComboBox.Items.Clear();
            foreach (var item in themes)
            {
                themesComboBox.Items.Add(item);
            }
            if(themesComboBox.Items.Count != 0) themesComboBox.Enabled = true;

            CheckFully();
        }

        private void CheckFully()
        {
            if(groupsComboBox.SelectedIndex != -1 && subjectsComboBox.SelectedIndex != -1 && themesComboBox.SelectedIndex != -1)
            {
                saveButton.Enabled = true;
            }
            else
            {
                saveButton.Enabled = false;
            }
        }

        private void ThemesComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            CheckFully();
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            CreateNewReport();
            saveButton.Enabled = false;
        }

        private void CreateNewReport()
        {
            string pathToTemplate = Path.Combine(Directory.GetCurrentDirectory(), "Templates\\Report.xlsx");
            var excel = new Excel.Application();

            Excel.Workbooks workbooks = excel.Workbooks;

            Excel.Workbook workbook = workbooks.Open(pathToTemplate);

            var worksheet = workbook.Worksheets[1];

            worksheet.Cells[5, "C"] = (themesComboBox.SelectedItem as Theme).Name;
            worksheet.Cells[7, "C"] = (groupsComboBox.SelectedItem as Group).Name;

            var selectedTheme = themesComboBox.SelectedItem as Theme;
            var selectedGroup = groupsComboBox.SelectedItem as Group;
            List<Models.Journal> journals = DatabaseHelper.GetJournalsByGroupId(selectedTheme, selectedGroup);

            int row = 10;
            double Average = 0;

            for (int i = 0; i < journals.Count; i++, row++)
            {
                worksheet.Cells[row, "A"].Value = i + 1;
                worksheet.Cells[row, "A"].Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
                worksheet.Cells[row, "A"].Borders.Weight = 2d;

                worksheet.Cells[row, "B"].Value = journals[i].client.surname;
                worksheet.Cells[row, "B"].Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
                worksheet.Cells[row, "B"].Borders.Weight = 2d;

                worksheet.Cells[row, "C"].Value = journals[i].client.name;
                worksheet.Cells[row, "C"].Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
                worksheet.Cells[row, "C"].Borders.Weight = 2d;

                worksheet.Cells[row, "D"].Value = journals[i].mark;
                worksheet.Cells[row, "D"].Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
                worksheet.Cells[row, "D"].Borders.Weight = 2d;

                Average += journals[i].mark;
            }

            var startCell = worksheet.Cells[10, "A"];
            var endCell = worksheet.Cells[row - 1, "D"];

            worksheet.Range[startCell, endCell].BorderAround(Excel.XlLineStyle.xlContinuous, Excel.XlBorderWeight.xlMedium);

            worksheet.Cells[row, "C"].Value = "Средний балл";
            worksheet.Cells[row, "D"].Value = Average / journals.Count;
            worksheet.Range[worksheet.Cells[row, "C"], worksheet.Cells[row, "D"]].Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
            worksheet.Range[worksheet.Cells[row, "C"], worksheet.Cells[row, "D"]].Borders.Weight = 2d;
            worksheet.Range[worksheet.Cells[row, "C"], worksheet.Cells[row, "D"]].BorderAround(Excel.XlLineStyle.xlContinuous, Excel.XlBorderWeight.xlMedium);

            row += 2;
            worksheet.Cells[row, "C"].Value = "Дата";
            worksheet.Cells[row, "D"].Value = $"{DateTime.Now.ToString("dd/MM/yyyy")}";

            endCell = worksheet.Cells[row, "D"];
            
            worksheet.Range[startCell, endCell].Font.Name = "Times New Roman";
            worksheet.Range[startCell, endCell].Font.Size = 12;
            worksheet.Range[startCell, endCell].Style.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
            worksheet.Range[startCell, endCell].Style.VerticalAlignment = Excel.XlVAlign.xlVAlignCenter;

            string addition = reportTypeComboBox.SelectedItem.ToString();

            if (!Directory.Exists("Reports"))
            {
                Directory.CreateDirectory("Reports");
            }
            
            workbook.SaveAs(Path.Combine(Directory.GetCurrentDirectory(), $"Reports\\Report_{addition}_{DateTime.Now.ToString("dd_MM_yyyy_HH_mm_ss")}.xlsx"));

            excel.Visible = true;
        }

        private void groupsComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            CheckFully();
        }
    }
}
