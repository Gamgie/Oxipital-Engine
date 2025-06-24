# Oxipital - Real-Time Particle Engine

![Occipital Logo](https://github.com/Gamgie/Oxipital-Legacy/blob/master/Communication/Oxipital.Logo.png)

**Oxipital** is an innovative particle engine designed to quickly and easily create beautiful particle effects, whether for real-time performances or pre-calculated creations.

## 🎯 Key Features

- **Unity 3D Based** - Optimized performance and high-quality rendering
- **Chataigne Control** - Intuitive interface with Benjamin Kuperberg's open source software
- **TouchDesigner Post-processing** - Advanced visual effects integration
- **Real-time** - Perfect for live performances and interactive installations

## 🚀 Installation and Launch

### Prerequisites
- [Chataigne](https://benjamin.kuperberg.fr/chataigne/en) by Benjamin Kuperberg
- TouchDesigner (optional, for post-processing)

### Quick Start

1. Download and install Chataigne
2. Open the Oxipital project folder
3. Navigate to the `chataigne/` folder
4. Launch Chataigne
5. In the Chataigne interface, click on the **Camera** tab
6. Click the "OXIPITAL EXE" button to start the program
7. Use Spout to use Oxipital in whatever other software

## 📖 User Guide

Oxipital is organized around three main concepts:

### 🎥 Camera

The Camera tab allows you to control your scene's viewpoint:

![Camera Interface](./Resources/Images/Camera_tab_v3.0.png)

#### Camera Modes

**Orbital Mode**
- Automatic rotation around the scene center
- Configurable rotation speed
- Continuous circular view

**Spaceship**
- Free camera control like a spaceship
- 6DOF navigation (6 degrees of freedom)
- Manual mode available via bottom-right control

#### Quick Controls
- **Predefined positions**: Top, Down, Left, Right
- **Random position**: Random button for spontaneous views
- **Focus center**: Define camera look-at point
- **Built-in timer**: Time tracking for your sessions

### ⚡ Orbs (Particle Emitters)

Orbs are the heart of Oxipital's particle system:

![Orbs Interface](./Resources/Images/Orb_Tab_v3.0.png)

#### Orb Parameters
- **Emission intensity** - Control the number of generated particles
- **Particle size** - Individual element dimensions
- **Emission shapes** - Multiple available geometries (sphere, cube, cylinder, etc.)
- **Presets** - Save and load system (beta feature)

#### Macro System
- **Grouped control** - One macro can control multiple parameters simultaneously
- **MIDI compatibility** - Connection with MIDI controllers
- **Sensors** - External sensor integration
- **Automation** - Parameter sequencing and automation

### 🧹 Placement System (Ballet)

The Ballet system allows positioning objects in 3D space:

#### Placement Patterns
- **Line** - Linear alignment of emitters
- **Circle** - Circular arrangement
- **Random** - Random placement
- **Mixed** - Combination of multiple patterns

#### Placement Parameters
- **Count** - Number of orbs to place
- **Speed** - Pattern movement speed
- **Rot** - Pattern orientation
- **Dancer Rotation** - rotation of an element of the ballet
- **Look At Mode** - either look at center,  position or the dancer rotation
- **Pattern Size** - Overall formation size

### 🌊 Forces

The Forces tab enables complex interactions between particles:

![Forces Interface](./Resources/Images/Force_Tab_v3.0.png)

#### Force Types
- **Radial** - Attracts / Push particles toward a point
- **OrthoRadial** - rotate particles around center
- **Linear** - like gravity for example (you can be much more creative than gravity with Oxipital
- **Orthoaxial** - rotate particle around an axis _ Not implemented yet
- **Axial** - attract particles along an axe
- **Turbulence** & **Perlin** - Adds chaos and unpredictability

#### Force Parameters
- **Size** - Force influence zone
- **Internal intensity** - Power inside size
- **External intensity** - Power outside size
- **Placement system** - Uses the same Balelt system as orbs

## 🎛️ Chataigne Integration

Oxipital leverages the power of [Chataigne](https://benjamin.kuperberg.fr/chataigne/en) to provide:

- Intuitive user interface
- Real-time control via OSC/MIDI
- Extensible module system
- Integration with other creative software

## 🎨 TouchDesigner Post-Processing

Add an extra dimension to your creations by integrating TouchDesigner for:

- Advanced visual effects
- Real-time image processing
- Multi-layer composition

## 🤝 Contributing

Contributions are welcome! Feel free to:

1. Fork the project
2. Create a branch for your feature
3. Commit your changes
4. Push to the branch
5. Open a Pull Request

## 📄 License

This project is licensed under the [MIT License](LICENSE).

## 🙏 Acknowledgments

- [Benjamin Kuperberg](https://benjamin.kuperberg.fr/) for Chataigne
- The Unity 3D community
- The TouchDesigner community

## 📞 Contact

For any questions or suggestions, feel free to open an issue or contact me directly.

---

**Oxipital** - Create, Control, Captivate ✨
