using System.Linq;
using EnvVar.Models;
using EnvVar.Services;
using Xunit;

namespace EnvVar.Tests.Models;

public class VariableEditorModelTests
{
    [Fact]
    public void LoadFrom_ShouldSetProperties()
    {
        var model = new VariableEditorModel();
        var entry = new EnvironmentVariableEntry
        {
            Name = "TEST_VAR",
            Value = "Value1;Value2",
            Level = EnvironmentVariableLevel.User,
            Alias = "Test Alias",
            Description = "Test Description",
            IsWellKnown = false
        };

        model.LoadFrom(entry);

        Assert.False(model.IsNew);
        Assert.Equal("TEST_VAR", model.Name);
        Assert.Equal("Value1;Value2", model.Value);
        Assert.Equal(EnvironmentVariableLevel.User, model.Level);
        Assert.Equal("Test Alias", model.Alias);
        Assert.Equal("Test Description", model.Description);
        Assert.Equal(2, model.EditableValues.Count);
    }

    [Fact]
    public void HasChanges_ShouldDetectChanges()
    {
        var model = new VariableEditorModel();
        var entry = new EnvironmentVariableEntry
        {
            Name = "TEST",
            Value = "VAL",
            Level = EnvironmentVariableLevel.User
        };
        model.LoadFrom(entry);

        Assert.False(model.HasChanges);

        model.Value = "NEW_VAL";
        Assert.True(model.HasChanges);

        model.Value = "VAL";
        Assert.False(model.HasChanges);

        model.Level = EnvironmentVariableLevel.System;
        Assert.True(model.HasChanges);
    }

    [Fact]
    public void DeduplicateValue_ShouldRemoveDuplicates()
    {
        var model = new VariableEditorModel();
        model.Value = "path1;path2;path1;PATH2";
        
        model.DeduplicateValue();

        Assert.Equal("path1;path2", model.Value);
    }

    [Fact]
    public void AddValueItem_ShouldUpdateValue()
    {
        var model = new VariableEditorModel();
        model.Value = "A;B";
        
        model.AddValueItem(-1); // Add to end
        model.EditableValues.Last().Value = "C";

        Assert.Equal("A;B;C", model.Value);
    }

    [Fact]
    public void MoveValueItem_ShouldUpdateValue()
    {
        var model = new VariableEditorModel();
        model.Value = "A;B;C";

        model.MoveValueItemUp(1); // Move B up

        Assert.Equal("B;A;C", model.Value);
    }

    [Fact]
    public void SortValues_ShouldUpdateValue()
    {
        var model = new VariableEditorModel();
        model.Value = "C;A;B";

        model.SortValuesAscending();

        Assert.Equal("A;B;C", model.Value);
    }
}
