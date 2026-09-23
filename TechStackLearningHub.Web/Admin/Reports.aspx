<%@ Page Title="Reports" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Reports.aspx.cs" Inherits="TechStackLearningHub.Web.Admin.Reports" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <h1>Reports</h1>

    <div class="row mb-4">
        <div class="col-md-5">
            <label class="form-label" for="ddlCourse">Filter by course</label>
            <asp:DropDownList ID="ddlCourse" runat="server" CssClass="form-select" AutoPostBack="true" OnSelectedIndexChanged="ddlCourse_SelectedIndexChanged" />
        </div>
    </div>

    <h3>Quiz results</h3>
    <asp:GridView ID="grdResults" runat="server" AutoGenerateColumns="false" CssClass="table table-striped mb-4" EmptyDataText="No quiz attempts for this filter.">
        <Columns>
            <asp:BoundField DataField="Username" HeaderText="Student" />
            <asp:BoundField DataField="CourseName" HeaderText="Course" />
            <asp:BoundField DataField="ModuleTitle" HeaderText="Module" />
            <asp:BoundField DataField="QuizTitle" HeaderText="Quiz" />
            <asp:TemplateField HeaderText="Score">
                <ItemTemplate><%# Eval("Score") %>%</ItemTemplate>
            </asp:TemplateField>
            <asp:TemplateField HeaderText="Result">
                <ItemTemplate>
                    <asp:Label runat="server" Text="Passed" CssClass="badge text-bg-success" Visible='<%# (bool)Eval("IsPassed") %>' />
                    <asp:Label runat="server" Text="Not passed" CssClass="badge text-bg-danger" Visible='<%# !(bool)Eval("IsPassed") %>' />
                </ItemTemplate>
            </asp:TemplateField>
            <asp:BoundField DataField="AttemptDate" HeaderText="Date" DataFormatString="{0:g}" />
        </Columns>
    </asp:GridView>

    <asp:Panel ID="pnlProgress" runat="server">
        <h3>Course completion</h3>
        <asp:GridView ID="grdProgress" runat="server" AutoGenerateColumns="false" CssClass="table table-striped" EmptyDataText="No students have progress in this course yet.">
            <Columns>
                <asp:BoundField DataField="Username" HeaderText="Student" />
                <asp:BoundField DataField="CourseName" HeaderText="Course" />
                <asp:TemplateField HeaderText="Completion">
                    <ItemTemplate><%# Eval("PercentComplete") %>%</ItemTemplate>
                </asp:TemplateField>
            </Columns>
        </asp:GridView>
    </asp:Panel>
</asp:Content>