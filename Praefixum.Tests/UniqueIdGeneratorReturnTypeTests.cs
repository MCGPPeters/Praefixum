using Xunit;

namespace Praefixum.Tests;

/// <summary>
/// Tests that verify the source generator preserves return types correctly
/// when methods have [UniqueId] parameters
/// </summary>
public class UniqueIdGeneratorReturnTypeTests
{
    [Fact]
    public void StringReturnType_IsPreserved()
    {
        // Act
        var result = TestHelpers.GetStringWithUniqueId();
        
        // Assert
        Assert.IsType<string>(result);
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public void IntReturnType_IsPreserved()
    {
        // Act
        var result = TestHelpers.GetIntWithUniqueId();
        
        // Assert
        Assert.IsType<int>(result);
    }

    [Fact]
    public void BoolReturnType_IsPreserved()
    {
        // Act
        var result = TestHelpers.GetBoolWithUniqueId();
        
        // Assert
        Assert.IsType<bool>(result);
    }

    [Fact]
    public void VoidReturnType_IsPreserved()
    {
        // Act & Assert (should not throw)
        TestHelpers.DoSomethingWithUniqueId();
        
        // Verify the ID was generated and stored
        Assert.NotNull(TestHelpers.LastGeneratedId);
        Assert.NotEmpty(TestHelpers.LastGeneratedId);
    }

    [Fact]
    public void DoubleReturnType_IsPreserved()
    {
        // Act
        var result = TestHelpers.GetDoubleWithUniqueId();
        
        // Assert
        Assert.IsType<double>(result);
    }

    [Fact]
    public void NullableStringReturnType_IsPreserved()
    {
        // Act
        var result1 = TestHelpers.GetNullableStringWithUniqueId();
        var result2 = TestHelpers.GetNullableStringWithUniqueId("");
        
        // Assert
        Assert.NotNull(result1); // Should get generated ID
        Assert.Null(result2); // Should return null for empty string
    }

    [Fact]
    public void CustomTypeReturnType_IsPreserved()
    {
        // Act
        var result = TestHelpers.GetCustomTypeWithUniqueId();
        
        // Assert
        Assert.IsType<TestHelpers.TestResult>(result);
        Assert.NotNull(result.Value);
        Assert.NotEmpty(result.Value);
    }

    [Fact]
    public void GenericListReturnType_IsPreserved()
    {
        // Act
        var result = TestHelpers.GetListWithUniqueId();
        
        // Assert
        Assert.IsType<List<string>>(result);
        Assert.Single(result);
        Assert.NotNull(result[0]);
        Assert.NotEmpty(result[0]);
    }

    [Fact]
    public void ComplexGenericReturnType_IsPreserved()
    {
        // Act
        var result = TestHelpers.GetDictionaryWithUniqueId();
        
        // Assert
        Assert.IsType<Dictionary<string, int>>(result);
        Assert.Single(result);
        var kvp = result.First();
        Assert.NotNull(kvp.Key);
        Assert.NotEmpty(kvp.Key);
        Assert.Equal(kvp.Key.GetHashCode(), kvp.Value);
    }

    [Fact]
    public async Task AsyncTaskReturnType_IsPreserved()
    {
        // Act
        var result = await TestHelpers.GetStringAsyncWithUniqueId();
        
        // Assert
        Assert.IsType<string>(result);
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public async Task ValueTaskReturnType_IsPreserved()
    {
        // Act
        var result = await TestHelpers.GetIntValueTaskWithUniqueId();
        
        // Assert
        Assert.IsType<int>(result);
    }

    [Fact]
    public void NullableCustomTypeReturnType_IsPreserved()
    {
        // Act
        var result1 = TestHelpers.GetNullableCustomTypeWithUniqueId();
        var result2 = TestHelpers.GetNullableCustomTypeWithUniqueId("");
        
        // Assert
        Assert.IsType<TestHelpers.TestResult>(result1);
        Assert.NotNull(result1);
        Assert.Null(result2);
    }

    [Fact]
    public void MethodsWithUniqueId_ActuallyUseGeneratedIds()
    {
        // Test that methods with [UniqueId] actually use the interceptor-generated IDs
        
        // Act
        var id1 = TestHelpers.GetIdForElement();
        var id2 = TestHelpers.GetIdForElement();
        var hash1 = TestHelpers.GetHashFromId();
        var hash2 = TestHelpers.GetHashFromId();
        
        // Assert
        Assert.NotEqual(id1, id2); // IDs should be unique
        Assert.NotEqual(hash1, hash2); // Hashes should be different because IDs are different
        Assert.True(TestHelpers.IsValidId()); // Should always be valid when generated
    }

    [Fact]
    public void DifferentFormatParameters_GenerateDifferentIds()
    {
        // Act - using methods with different UniqueId formats
        var guidBasedHash = TestHelpers.GetHashFromId(); // Uses HtmlId format
        var validityCheck = TestHelpers.IsValidId(); // Uses Guid format
        
        // These should work without issues even with different formats
        Assert.IsType<int>(guidBasedHash);
        Assert.IsType<bool>(validityCheck);
        Assert.True(validityCheck); // Generated GUID should always be valid
    }
}