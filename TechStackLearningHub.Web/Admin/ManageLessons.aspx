<%@ Page Title="Manage Lessons" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="ManageLessons.aspx.cs" Inherits="TechStackLearningHub.Web.Admin.ManageLessons" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <h1>Manage Lessons</h1>

    <div class="row g-3 mb-4">
        <div class="col-md-4">
            <label class="form-label" for="ddlCourse">Course</label>
            <asp:DropDownList ID="ddlCourse" runat="server" CssClass="form-select" AutoPostBack="true" OnSelectedIndexChanged="ddlCourse_SelectedIndexChanged" />
        </div>
        <div class="col-md-4">
            <label class="form-label" for="ddlModule">Module</label>
            <asp:DropDownList ID="ddlModule" runat="server" CssClass="form-select" AutoPostBack="true" OnSelectedIndexChanged="ddlModule_SelectedIndexChanged" />
        </div>
    </div>

    <asp:Panel ID="pnlWorkspace" runat="server" Visible="false">
        <asp:GridView ID="grdLessons" runat="server" AutoGenerateColumns="false" CssClass="table table-striped mb-4"
            DataKeyNames="LessonID" OnRowCommand="grdLessons_RowCommand" EmptyDataText="No lessons in this module yet.">
            <Columns>
                <asp:TemplateField HeaderText="#">
                    <ItemTemplate><%# Eval("LessonOrder") %></ItemTemplate>
                </asp:TemplateField>
                <asp:BoundField DataField="LessonTitle" HeaderText="Title" />
                <asp:TemplateField>
                    <ItemTemplate>
                        <asp:LinkButton runat="server" Text="Edit" CommandName="EditLesson" CommandArgument='<%# Eval("LessonID") %>' CssClass="btn btn-sm btn-outline-primary" />
                        <asp:LinkButton runat="server" Text="Delete" CommandName="DeleteLesson" CommandArgument='<%# Eval("LessonID") %>' CssClass="btn btn-sm btn-outline-danger"
                            OnClientClick="return confirm('Delete this lesson? Progress on it will be removed.');" />
                    </ItemTemplate>
                </asp:TemplateField>
            </Columns>
        </asp:GridView>

        <div class="card">
            <div class="card-body">
                <h5 class="card-title" id="lblEditorHeading" runat="server">New lesson</h5>
                <asp:HiddenField ID="hidLessonId" runat="server" />
                <div class="row g-3">
                    <div class="col-12">
                        <label class="form-label" for="txtLessonTitle">Lesson title</label>
                        <asp:TextBox ID="txtLessonTitle" runat="server" CssClass="form-control" />
                    </div>
                    <div class="col-12">
                        <label class="form-label" for="txtContentHtml">Lesson content (HTML allowed)</label>
                        <asp:TextBox ID="txtContentHtml" runat="server" TextMode="MultiLine" Rows="8" CssClass="form-control"
                            placeholder="Introductory paragraphs, code samples, headings..." />
                    </div>
                    <div class="col-md-6">
                        <label class="form-label" for="txtVideoUrl">Video URL (YouTube embed / Vimeo player)</label>
                        <asp:TextBox ID="txtVideoUrl" runat="server" CssClass="form-control" Placeholder="https://www.youtube.com/embed/..." />
                    </div>
                    <div class="col-md-6">
                        <label class="form-label" for="fupNotes">Notes file (PDF/DOCX/TXT)</label>
                        <asp:FileUpload ID="fupNotes" runat="server" CssClass="form-control" />
                        <asp:Label ID="lblCurrentNotes" runat="server" CssClass="form-text d-block" />
                    </div>
                </div>
                <asp:Label ID="lblMessage" runat="server" CssClass="text-danger d-block mt-2" Visible="false" />
                <div class="mt-3">
                    <asp:Button ID="btnSave" runat="server" Text="Save lesson" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                    <asp:Button ID="btnCancelEdit" runat="server" Text="Cancel" CssClass="btn btn-outline-secondary" OnClick="btnCancelEdit_Click" Visible="false" />
                </div>
            </div>
        </div>
    </asp:Panel>
</asp:Content>