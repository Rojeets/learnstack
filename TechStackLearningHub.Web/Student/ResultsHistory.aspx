<%@ Page Title="Quiz History" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="ResultsHistory.aspx.cs" Inherits="TechStackLearningHub.Web.Student.ResultsHistory" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <h1>Quiz History</h1>
    <p class="lead">Your past quiz attempts.</p>

    <asp:GridView ID="grdHistory" runat="server" AutoGenerateColumns="false" CssClass="table table-striped" EmptyDataText="You have not taken any quizzes yet.">
        <Columns>
            <asp:BoundField DataField="QuizTitle" HeaderText="Quiz" />
            <asp:BoundField DataField="ModuleTitle" HeaderText="Module" />
            <asp:TemplateField HeaderText="Score">
                <ItemTemplate>
                    <span class="fw-semibold"><%# Eval("Score") %>%</span>
                </ItemTemplate>
            </asp:TemplateField>
            <asp:BoundField DataField="AttemptDate" HeaderText="Date" DataFormatString="{0:g}" />
            <asp:TemplateField HeaderText="Result">
                <ItemTemplate>
                    <asp:Label runat="server" Text="Passed" CssClass="badge text-bg-success"
                        Visible='<%# (bool)Eval("IsPassed") %>' />
                    <asp:Label runat="server" Text="Not passed" CssClass="badge text-bg-danger"
                        Visible='<%# !(bool)Eval("IsPassed") %>' />
                </ItemTemplate>
            </asp:TemplateField>
        </Columns>
    </asp:GridView>
</asp:Content>