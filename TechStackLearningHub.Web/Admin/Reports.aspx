<%@ Page Title="Reports" Language="C#" MasterPageFile="~/Masterpages/Admin.Master" AutoEventWireup="true" CodeBehind="Reports.aspx.cs" Inherits="TechStackLearningHub.Web.Admin.Reports" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <div class="row mb-4">
        <div class="col-md-5">
            <asp:Label runat="server" AssociatedControlID="ddlCourse" CssClass="form-label" Text="Filter by course" />
            <asp:DropDownList ID="ddlCourse" runat="server" CssClass="form-select" AutoPostBack="true" OnSelectedIndexChanged="ddlCourse_SelectedIndexChanged" />
        </div>
    </div>

    <h2 class="h5">Quiz results</h2>
    <asp:GridView ID="grdResults" runat="server" AutoGenerateColumns="false" CssClass="table table-hover mb-0" EmptyDataText="No quiz attempts for this filter." AllowPaging="true" PageSize="10" PagerStyle-CssClass="grid-pager">
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
        <PagerSettings Mode="Numeric" Position="Bottom" PreviousPageText="&#8592; Newer" NextPageText="Older &#8594;" />
    </asp:GridView>

    <asp:Panel ID="pnlProgress" runat="server">
        <h2 class="h5">Course completion</h2>
        <asp:GridView ID="grdProgress" runat="server" AutoGenerateColumns="false" CssClass="table table-hover mb-0" EmptyDataText="No students have progress in this course yet." AllowPaging="true" PageSize="10" PagerStyle-CssClass="grid-pager">
            <Columns>
                <asp:BoundField DataField="Username" HeaderText="Student" />
                <asp:BoundField DataField="CourseName" HeaderText="Course" />
                <asp:TemplateField HeaderText="Completion">
                    <ItemTemplate><%# Eval("PercentComplete") %>%</ItemTemplate>
                </asp:TemplateField>
            </Columns>
            <PagerSettings Mode="Numeric" Position="Bottom" PreviousPageText="&#8592; Newer" NextPageText="Older &#8594;" />
        </asp:GridView>
    </asp:Panel>
</asp:Content>