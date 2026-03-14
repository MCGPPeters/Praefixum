namespace Praefixum.Tests;

/// <summary>
/// Tests that verify the source generator preserves return types correctly
/// when methods have [UniqueId] parameters
/// </summary>
public class UniqueIdGeneratorReturnTypeTests
{
    [Test]
    public async Task StringReturnType_IsPreserved()
    {
        // Act
        var result = TestHelpers.GetStringWithUniqueId();
        
        // Assert
        await Assert.That(result).IsTypeOf<string>();
        await Assert.That(result).IsNotNull();
        await Assert.That(result).IsNotEmpty();
    }

    [Test]
    public async Task IntReturnType_IsPreserved()
    {
        // Act
        var result = TestHelpers.GetIntWithUniqueId();
        
        // Assert
        await Assert.That(result).IsTypeOf<int>();
    }

    [Test]
    public async Task BoolReturnType_IsPreserved()
    {
        // Act
        var result = TestHelpers.GetBoolWithUniqueId();
        
        // Assert
        await Assert.That(result).IsTypeOf<bool>();
    }

    [Test]
    public async Task VoidReturnType_IsPreserved()
    {
        // Act & Assert (should not throw)
        TestHelpers.DoSomethingWithUniqueId();
        
        // Verify the ID was generated and stored
        await Assert.That(TestHelpers.LastGeneratedId).IsNotNull();
        await Assert.That(TestHelpers.LastGeneratedId!).IsNotEmpty();
    }

    [Test]
    public async Task DoubleReturnType_IsPreserved()
    {
        // Act
        var result = TestHelpers.GetDoubleWithUniqueId();
        
        // Assert
        await Assert.That(result).IsTypeOf<double>();
    }

    [Test]
    public async Task NullableStringReturnType_IsPreserved()
    {
        // Act
        var result1 = TestHelpers.GetNullableStringWithUniqueId();
        var result2 = TestHelpers.GetNullableStringWithUniqueId("");
        
        // Assert
        await Assert.That(result1).IsNotNull(); // Should get generated ID
        await Assert.That(result2).IsNull(); // Should return null for empty string
    }

    [Test]
    public async Task CustomTypeReturnType_IsPreserved()
    {
        // Act
        var result = TestHelpers.GetCustomTypeWithUniqueId();
        
        // Assert
        await Assert.That(result).IsTypeOf<TestHelpers.TestResult>();
        await Assert.That(result.Value).IsNotNull();
        await Assert.That(result.Value).IsNotEmpty();
    }

    [Test]
    public async Task GenericListReturnType_IsPreserved()
    {
        // Act
        var result = TestHelpers.GetListWithUniqueId();
        
        // Assert
        await Assert.That(result).IsTypeOf<List<string>>();
        await Assert.That(result).HasSingleItem();
        await Assert.That(result[0]).IsNotNull();
        await Assert.That(result[0]).IsNotEmpty();
    }

    [Test]
    public async Task ComplexGenericReturnType_IsPreserved()
    {
        // Act
        var result = TestHelpers.GetDictionaryWithUniqueId();
        
        // Assert
        await Assert.That(result).IsTypeOf<Dictionary<string, int>>();
        await Assert.That(result).HasSingleItem();
        var kvp = result.First();
        await Assert.That(kvp.Key).IsNotNull();
        await Assert.That(kvp.Key).IsNotEmpty();
        await Assert.That(kvp.Value).IsEqualTo(kvp.Key.GetHashCode());
    }

    [Test]
    public async Task AsyncTaskReturnType_IsPreserved()
    {
        // Act
        var result = await TestHelpers.GetStringAsyncWithUniqueId();
        
        // Assert
        await Assert.That(result).IsTypeOf<string>();
        await Assert.That(result).IsNotNull();
        await Assert.That(result).IsNotEmpty();
    }

    [Test]
    public async Task ValueTaskReturnType_IsPreserved()
    {
        // Act
        var result = await TestHelpers.GetIntValueTaskWithUniqueId();
        
        // Assert
        await Assert.That(result).IsTypeOf<int>();
    }

    [Test]
    public async Task NullableCustomTypeReturnType_IsPreserved()
    {
        // Act
        var result1 = TestHelpers.GetNullableCustomTypeWithUniqueId();
        var result2 = TestHelpers.GetNullableCustomTypeWithUniqueId("");
        
        // Assert
        await Assert.That(result1).IsTypeOf<TestHelpers.TestResult>();
        await Assert.That(result1).IsNotNull();
        await Assert.That(result2).IsNull();
    }

    [Test]
    public async Task MethodsWithUniqueId_ActuallyUseGeneratedIds()
    {
        // Test that methods with [UniqueId] actually use the interceptor-generated IDs
        
        // Act
        var id1 = TestHelpers.GetIdForElement();
        var id2 = TestHelpers.GetIdForElement();
        var hash1 = TestHelpers.GetHashFromId();
        var hash2 = TestHelpers.GetHashFromId();
        
        // Assert
        await Assert.That(id1).IsNotEqualTo(id2); // IDs should be unique
        await Assert.That(hash1).IsNotEqualTo(hash2); // Hashes should be different because IDs are different
        await Assert.That(TestHelpers.IsValidId()).IsTrue(); // Should always be valid when generated
    }

    [Test]
    public async Task DifferentFormatParameters_GenerateDifferentIds()
    {
        // Act - using methods with different UniqueId formats
        var guidBasedHash = TestHelpers.GetHashFromId(); // Uses HtmlId format
        var validityCheck = TestHelpers.IsValidId(); // Uses Guid format
        
        // These should work without issues even with different formats
        await Assert.That(guidBasedHash).IsTypeOf<int>();
        await Assert.That(validityCheck).IsTypeOf<bool>();
        await Assert.That(validityCheck).IsTrue(); // Generated GUID should always be valid
    }
}
