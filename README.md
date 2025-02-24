# BuildYourMVPDemo

BuildYourMVPDemo is a project to demonstrate the process of building a Minimum Viable Product (MVP) using various technologies.

## Technologies Used

- **C#**: 65.8%
- **HTML**: 13.4%
- **CSS**: 8.9%
- **JavaScript**: 5.5%
- **Dockerfile**: 3%
- **TSQL**: 2.2%
- **Other**: 1.2%

## Project Structure

- **APIGateway**: This directory contains the API Gateway project which routes requests to the appropriate microservices.
- **Frontend**: This directory contains the frontend project built with HTML, CSS, and JavaScript.
- **Backend**: This directory contains the backend services implemented in C#.
- **Tests**: This directory contains the test projects for various components of the system.

## Getting Started

To get a local copy up and running follow these simple steps.

### Prerequisites

- [.NET SDK](https://aka.ms/dotnet/download) (version that supports .NET 9.0 or later)
- [Docker](https://www.docker.com/get-started)

### Installation

1. Clone the repo
   ```sh
   git clone https://github.com/fatihdurgut/BuildYourMVPDemo.git
   ```
2. Install .NET dependencies
   ```sh
   cd BuildYourMVPDemo
   dotnet restore
   ```

### Running the Application

1. Start the backend services
   ```sh
   dotnet run --project Backend/Backend.csproj
   ```
2. Start the API Gateway
   ```sh
   dotnet run --project APIGateway/APIGateway.csproj
   ```
3. Start the frontend application
   ```sh
   dotnet run --project Frontend/Frontend.csproj
   ```

### Running Tests

```sh
dotnet test
```

## Contributing

Contributions are what make the open source community such an amazing place to learn, inspire, and create. Any contributions you make are **greatly appreciated**.

1. Fork the Project
2. Create your Feature Branch (`git checkout -b feature/AmazingFeature`)
3. Commit your Changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the Branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## License

Distributed under the MIT License. See `LICENSE.txt` for more information.

## Contact

Fatih Durgut - [@fatihdurgut](https://github.com/fatihdurgut)

Project Link: [https://github.com/fatihdurgut/BuildYourMVPDemo](https://github.com/fatihdurgut/BuildYourMVPDemo)
