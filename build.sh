#!/usr/bin/env bash
set -e

PROJECT="HydrogenOrbital"
PYTHON_DEPS="numpy matplotlib"

build() {
    echo "Building $PROJECT..."
    dotnet build -c Release --nologo -v quiet
    echo "Done."
}

run() {
    echo "Running $PROJECT..."
    dotnet run -c Release --no-build
}

plot() {
    echo "Installing Python dependencies..."
    pip install $PYTHON_DEPS -q
    echo "Generating orbital plot..."
    python3 plot_orbital.py
}

clean() {
    echo "Cleaning build artifacts..."
    rm -rf bin obj
    echo "Done."
}

case "${1:-}" in
    build)  build ;;
    run)    build && run ;;
    plot)   plot ;;
    clean)  clean ;;
    *)
        echo "Usage: ./build.sh [build|run|plot|clean]"
        echo ""
        echo "  build  — compile in Release mode"
        echo "  run    — build and launch simulation"
        echo "  plot   — generate 2D probability density plot (requires Python)"
        echo "  clean  — remove bin/ and obj/"
        ;;
esac
