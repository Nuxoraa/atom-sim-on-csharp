import numpy as np
import matplotlib.pyplot as plt
import matplotlib.colors as mcolors

def psi_2pz(r, cos_theta):
    return r * np.exp(-r / 2.0) * cos_theta

def probability_density(r, cos_theta):
    psi = psi_2pz(r, cos_theta)
    return psi ** 2

grid_size = 800
extent = 18.0
coords = np.linspace(-extent, extent, grid_size)
x, z = np.meshgrid(coords, coords)

r = np.sqrt(x**2 + z**2)
r = np.where(r < 1e-6, 1e-6, r)
cos_theta = z / r

density = probability_density(r, cos_theta)
density_norm = density / density.max()

fig, axes = plt.subplots(1, 2, figsize=(14, 6), facecolor="#04040c")
fig.suptitle("Hydrogen 2pz Orbital — Probability Density |ψ|²",
             color="white", fontsize=14, y=1.01)

colors_upper = [(0, 0, 0), (0.2, 0.05, 0.35), (1.0, 0.31, 0.86)]
colors_lower = [(0, 0, 0), (0.0, 0.1, 0.35), (0.24, 0.71, 1.0)]
cmap_upper = mcolors.LinearSegmentedColormap.from_list("upper", colors_upper)
cmap_lower = mcolors.LinearSegmentedColormap.from_list("lower", colors_lower)

combined_cmap_data = np.zeros((*density_norm.shape, 4))
upper_mask = z > 0
lower_mask = z <= 0

upper_rgba = cmap_upper(density_norm)
lower_rgba = cmap_lower(density_norm)

combined_cmap_data[upper_mask] = upper_rgba[upper_mask]
combined_cmap_data[lower_mask] = lower_rgba[lower_mask]

ax1 = axes[0]
ax1.set_facecolor("#04040c")
ax1.imshow(combined_cmap_data, extent=[-extent, extent, -extent, extent],
           origin="lower", interpolation="bilinear")
ax1.set_title("XZ cross-section (y=0)", color="white", fontsize=11)
ax1.set_xlabel("x (Bohr radii)", color="#aaaacc")
ax1.set_ylabel("z (Bohr radii)", color="#aaaacc")
ax1.tick_params(colors="#888899")
for spine in ax1.spines.values():
    spine.set_edgecolor("#333344")
ax1.axhline(0, color="#ffffff22", linewidth=0.5)
ax1.axvline(0, color="#ffffff22", linewidth=0.5)

ax2 = axes[1]
ax2.set_facecolor("#04040c")
r_vals = np.linspace(0, 18, 1000)
radial = r_vals**2 * np.exp(-r_vals)
radial_2pz = r_vals**4 * np.exp(-r_vals)
radial_2pz /= radial_2pz.max()

ax2.plot(r_vals, radial_2pz, color="#cc50e0", linewidth=2, label="2pz  R(r)²·r²")
ax2.fill_between(r_vals, radial_2pz, alpha=0.15, color="#cc50e0")
ax2.set_title("Radial probability distribution", color="white", fontsize=11)
ax2.set_xlabel("r (Bohr radii)", color="#aaaacc")
ax2.set_ylabel("P(r)", color="#aaaacc")
ax2.tick_params(colors="#888899")
ax2.legend(facecolor="#111122", edgecolor="#333344", labelcolor="white")
ax2.set_facecolor("#04040c")
for spine in ax2.spines.values():
    spine.set_edgecolor("#333344")
ax2.grid(True, color="#1a1a2e", linewidth=0.5)

plt.tight_layout()
plt.savefig("orbital_plot.png", dpi=150, bbox_inches="tight",
            facecolor="#04040c")
print("Saved: orbital_plot.png")
plt.show()
