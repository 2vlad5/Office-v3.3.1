# OfficeApp

## Overview

OfficeApp is a .NET-based office management application that appears to have both console and Windows Forms components. The project is built on .NET 8.0 and uses MySQL as its primary database solution. The application seems designed to handle office-related operations with a focus on user authentication and data management through a combination of console utilities and desktop interface components.

## User Preferences

Preferred communication style: Simple, everyday language.

## System Architecture

### Application Structure
The project follows a multi-project .NET solution architecture with separate console and desktop application components:

- **Console Application (OfficeApp.Console)**: Built on .NET 8.0, likely serves as a command-line utility or background service for administrative tasks
- **Desktop Application**: Windows Forms-based GUI application with user authentication features (LoginForm)

### Database Architecture
The system uses MySQL as the primary database with the following design decisions:

- **MySqlConnector**: Modern, high-performance MySQL driver chosen over the legacy MySql.Data connector
- **Version Strategy**: Uses MySqlConnector 2.4.0 with flexible versioning to allow minor updates
- **Connection Management**: Leverages Microsoft Extensions for dependency injection and logging abstractions

### Technology Stack
- **Runtime**: .NET 8.0 (latest LTS version for enhanced performance and security)
- **Database**: MySQL with MySqlConnector for optimal async performance
- **UI Framework**: Windows Forms for desktop interface
- **Dependency Management**: Microsoft Extensions for IoC container and logging abstractions

### Design Patterns
- **Dependency Injection**: Uses Microsoft.Extensions.DependencyInjection.Abstractions for loose coupling
- **Logging**: Implements Microsoft.Extensions.Logging.Abstractions for centralized logging strategy
- **Separation of Concerns**: Console and desktop applications are separated into distinct projects

### Build and Deployment
- **Target Framework**: .NET 8.0 for modern language features and performance improvements
- **Package Management**: NuGet with PackageReference style for better dependency resolution
- **Configuration**: Uses standard .NET configuration management with runtime configuration files

## External Dependencies

### Database Services
- **MySQL Database**: Primary data storage solution requiring MySQL server instance
- **MySqlConnector 2.4.0**: High-performance MySQL ADO.NET driver with async support

### Microsoft Extensions
- **Microsoft.Extensions.DependencyInjection.Abstractions 8.0.2**: Provides IoC container abstractions
- **Microsoft.Extensions.Logging.Abstractions 8.0.2**: Enables structured logging capabilities

### Runtime Dependencies
- **.NET 8.0 Runtime**: Requires .NET 8.0 runtime environment for application execution
- **Windows Forms**: Desktop UI framework dependency for GUI components

### Development Tools
- **NuGet Package Manager**: For dependency management and package restoration
- **MSBuild**: Build system for compilation and project management

## Recent Changes (September 2025)

### .NET 8.0 Framework Upgrade & Enhanced Warehouse Management (September 11, 2025)

#### Framework Modernization
- **Upgraded to .NET 8.0**: Updated from .NET 7.0 to the latest LTS version for improved performance and security
- **Project Compatibility**: Maintained full compatibility while adding modern language features and runtime improvements
- **Dependency Updates**: All packages updated to .NET 8.0 compatible versions

#### ProductForm Type Selection Enhancement
- **Type Selection ComboBox**: Added product type selection dropdown with dynamic loading from database
- **Type Management Integration**: Users can now add new types directly from the product form
- **Database Integration**: Full integration with office_storage_options table for type management

#### Advanced Warehouse Module Improvements

The warehouse module has been completely redesigned and optimized with the following improvements:

#### Database Schema Updates
- **New Table Integration**: Enhanced integration with `office_storage_options` table for dynamic configuration
- **Enable Filtering**: Added support for `enable=1` filtering to show only active options
- **Type Management**: Implemented product type management through option_type='type' entries
- **Group Management**: Dynamic group management through option_type='group' entries

#### Performance Optimizations
- **UI Performance**: Eliminated N+1 query problems in DataGridView cell formatting by preloading product types via efficient JOIN queries
- **Caching Strategy**: Added Type property to Product model for in-memory caching, preventing database calls during UI rendering
- **Optimized Queries**: Updated ProductRepository methods to use single JOIN queries instead of multiple database calls

#### Security Enhancements
- **Credential Management**: Removed hardcoded database credentials from console application
- **Environment Variables**: Implemented secure credential management using DATABASE_CONNECTION_STRING environment variable
- **Information Masking**: Added sensitive information masking in application logs for production safety

#### Special Product Type Handling
- **"Образ" Type Logic**: Implemented special handling for products with type "Образ" to disable stock level color highlighting
- **Conditional UI Behavior**: Products of type "Образ" now maintain standard colors regardless of stock quantity

#### New Repository Methods
- **GetProductType**: Retrieves product type by joining storage and options tables
- **IsProductTypeObraz**: Checks if product is of type "Образ" for UI conditional logic
- **AddGroupToOptions**: Dynamically adds new product groups to options table
- **RemoveGroupFromOptions**: Safely disables groups using enable flag instead of deletion

#### Technical Improvements
- **Schema Alignment**: Fixed critical JOIN queries to use correct field mappings (office_storage.name → office_storage_options.value)
- **Error Handling**: Enhanced error handling with proper exception management and user-friendly messages
- **Code Safety**: All queries use parameterized SQL to prevent injection attacks

### Latest GUI Enhancements (September 11, 2025)

#### Enhanced Filtering and Sorting System
- **Multi-Level Filtering**: Added comprehensive filtering by name, volume, status, and product type
- **Advanced Sorting**: Flexible column-based sorting with ascending/descending order options
- **Real-Time Search**: Dynamic search functionality across product names and volumes
- **Filter Management**: Clear all filters functionality with one-click reset

#### Mass Operations & Bulk Management
- **Checkbox Selection**: Interactive checkbox column for selecting multiple products
- **Mass Type Changes**: Bulk modification of product types for selected items
- **Mass Status Updates**: Batch status changes across multiple products
- **Bulk Deletion**: Safe mass deletion with confirmation prompts
- **Selection Management**: "Select All" functionality with automatic panel visibility

#### New DataGridView Features
- **Type Column Display**: Added "🏷️ Тип" column to show product types loaded from office_storage_options table
- **Interactive Selection**: Checkbox-based row selection for mass operations
- **Smart Notification System**: Implemented conditional notification button that appears only for products with `msg_send=true` AND `quantity<10`
- **Excel Export Capability**: Added "📊 Экспорт Excel" button with CSV export functionality including all product fields

#### Notification System Implementation
- **Conditional Button Logic**: Notification buttons (📧 Уведомить) only visible for eligible products meeting both criteria
- **Safe Click Handling**: Built-in validation prevents unauthorized notifications for ineligible products
- **Custom Input Dialog**: Replaced external Microsoft.VisualBasic dependency with custom WinForms email input dialog
- **Enhanced User Feedback**: Clear messages inform users about notification success/failure states

#### Export Features
- **CSV Format Support**: Full product data export in CSV format compatible with Excel
- **Comprehensive Data**: Export includes all fields: ID, Name, Volume, Quantity, Status, Group, Type, CountInTotal, AdditionalInfo, MsgSend
- **Safe Data Handling**: Proper CSV escaping for special characters and quotes
- **User-Friendly Interface**: SaveFileDialog with timestamp-based default filenames

#### Architecture Improvements
- **Button Visibility Control**: Fixed UseColumnTextForButtonValue logic for proper per-cell button display
- **Performance Optimization**: Efficient data loading prevents UI lag during rendering
- **Security Enhancement**: All user inputs validated and sanitized before processing
- **No External Dependencies**: Self-contained implementation without additional library requirements

### Update System & Security Enhancements (September 11, 2025)

#### Secure UpdateService Implementation
- **Version Validation**: Implemented strict regex-based version format validation to prevent path traversal attacks
- **Input Sanitization**: Added comprehensive input sanitization for all version strings and file paths
- **HTTPS Enforcement**: Mandatory HTTPS-only downloads with URL validation
- **File Integrity Verification**: SHA-256 hash checking for downloaded update files with automatic corrupted file removal
- **Size Limits**: 100MB file size cap with streaming progress tracking and cancellation support
- **Error Handling**: Comprehensive error handling with user-friendly security messaging

#### Enhanced UpdatesModule
- **Version Comparison**: Redesigned to show current application version vs database available version
- **Secure Download Interface**: Integration with UpdateService for safe update downloads
- **Progress Tracking**: Real-time download progress with cancellation capabilities
- **User Feedback**: Clear status messaging for download success/failure states

#### Product Type Management Improvements
- **Data Model Correction**: Fixed product type retrieval to use office_storage.product_type column instead of incorrect table joins
- **Transaction Safety**: All type management operations now use database transactions for data consistency
- **Type Assignment**: Added AssignTypeToProduct method for proper product-to-type relationships
- **CRUD Operations**: Enhanced add/update/remove type operations with proper error handling and user feedback
- **Warehouse Integration**: Added type management dialog in WarehouseModule with local input forms

#### Security Architecture Updates
- **Database Security**: Environment variable-based connection string management (DATABASE_CONNECTION_STRING)
- **SQL Injection Prevention**: All queries use parameterized SQL to prevent injection attacks
- **Input Validation**: Comprehensive validation for all user inputs with regex pattern matching
- **Error Containment**: Secure error handling that doesn't expose sensitive system information

#### Product Repository Enhancements
- **Complete Type Persistence**: Fixed AddProduct and UpdateProduct methods to properly save product_type column
- **Comprehensive Updates**: All product fields (type, group, status, additional info) now properly persist to database
- **Transaction Safety**: Enhanced error handling and parameter validation for all database operations
- **Mass Operation Support**: Repository methods optimized for bulk operations and batch updates

#### User Interface Improvements
- **Responsive Layout**: Enhanced panel management with proper sizing and positioning
- **Mass Actions Panel**: Dynamic visibility based on selection state
- **Improved UX**: Better button placement, clear labeling, and intuitive workflows
- **Visual Feedback**: Clear status messages and confirmation dialogs for all operations

#### Database Schema Requirements
- **office_app_settings**: Table for storing application update information and version control
- **office_storage.product_type**: Column for storing product type assignments (fully implemented)
- **office_storage_options**: Enhanced option management for types and groups with enable/disable functionality

#### Runtime Environment
- **Development Environment**: Configured for Linux/Replit development with proper dependency management
- **Production Deployment**: Designed for Windows environments with full WinForms support
- **Database Connectivity**: MySQL connector with secure connection string management