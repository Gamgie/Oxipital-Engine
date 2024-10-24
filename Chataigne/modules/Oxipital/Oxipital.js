
script.setExecutionTimeout(20);
script.setAutoRefreshEnvironment(false);

var numForceGroupsParam;;
var numOrbGroupsParam;;
var numMacrosParam;;

var macrosGroup;
var forceGroupsGroup;
var orbGroupsGroup;

var forces = [];
var orbGroups = [];
var macros = [];

//Unity links
var unityBallet = null;
var unityForcesManager = null;
var unityOrbsManager = null;
var unityForceGroupsParam = null;
var unityOrbGroupsParam = null;

var lastSyncTime = 0;

var macroActiveParams = [];

var paramPropLinks = {};

var danceGroupParameters = {
	"Patterns": {
		"Count": { "type": "float", "default": 1, "min": 1, "max": 10, "noMacro": true },
		"Pattern Size": { "type": "float", "default": 1, "min": 0, "max": 20 },
		"Pattern Size Spread": { "type": "float", "default": 0, "min": 0, "max": 1 },
		"Pattern Axis Spread": { "type": "float", "default": 0, "min": 0, "max": 1 },
		"Line Pattern Weight": { "type": "float", "default": 0, "min": 0, "max": 1, "customComponent": "LineDancePattern/weight" },
		"Line Pattern Speed Weight": { "type": "float", "default": 0, "min": 0, "max": 1, "customComponent": "LineDancePattern/speedWeight" },
		"Circle Pattern Weight": { "type": "float", "default": 1, "min": 0, "max": 1, "customComponent": "CircleDancePattern/weight" },
		"Circle Pattern Speed Weight": { "type": "float", "default": 1, "min": 0, "max": 1, "customComponent": "CircleDancePattern/speedWeight" },
		"N Body Pattern Weight": { "type": "float", "default": 0, "min": 0, "max": 1, "customComponent": "NBodyProblemPattern/weight" },
		"N Body Pattern Speed Weight": { "type": "float", "default": 0, "min": 0, "max": 1, "customComponent": "NBodyProblemPattern/speedWeight" }
	},
	"Animation": {
		"Pattern Speed": { "type": "float", "default": 0.1, "min": -1, "max": 1 },
		"Pattern Speed Random": { "type": "float", "default": 0, "min": 0, "max": 1 },
		"Pattern Time Offset": { "type": "float", "default": 0, "min": 0, "max": 1 }

	},
	"Dancer": {
		"Dancer Size": { "type": "float", "default": 1, "min": 0, "max": 20 },
		"Dancer Hyper Size": { "type": "float", "default": 0, "min": 0, "max": 1 },
		"Dancer Size Spread": { "type": "float", "default": 0, "min": 0, "max": 1 },
		"Dancer Weight Size Factor": { "type": "float", "default": 0, "min": 0, "max": 1 },
		"Dancer Intensity": { "type": "float", "default": 1, "min": 0, "max": 1 },
		"Dancer Weight Intensity Factor": { "type": "float", "default": 0, "min": 0, "max": 1 },
		"Dancer Rotation": { "type": "p3d", "default": [0, 0, 0] },
		"Dancer Look At": { "type": "p3d", "default": [0, 1, 0] },
		"Dancer Look At Mode": { "type": "float", "default": 0, "min": 0, "max": 2 }
	}
};

var forceGroupParameters = {
	"General": {
		"Force Factor Inside": { "type": "float", "default": 1, "min": 0, "max": 1 },
		"Force Factor Outside": { "type": "float", "default": 0, "min": 0, "max": 1 }
	},
	"Radial": {
		"Radial Intensity": { "type": "float", "default": 0, "min": 0, "max": 1 },
		"Radial Frequency": { "type": "float", "default": 0, "min": 0, "max": 1 }
	},
	"Axial": {
		"Axial Intensity": { "type": "float", "default": 0, "min": 0, "max": 1 },
		"Axis Multiplier": { "type": "p3d", "default": [0, 1, 0] },
		"Axial Factor": { "type": "float", "default": 1, "min": 1, "max": 3 },
		"Axial Frequency": { "type": "p3d", "default": [0, 0, 0] }
	},
	"Linear": {
		"Linear Intensity": { "type": "float", "default": 0, "min": 0, "max": 1 }
	},
	"Orthoradial": {
		"Ortho Intensity": { "type": "float", "default": 0, "min": 0, "max": 1 },
		"Ortho Inner Radius": { "type": "float", "default": 0.5, "min": 0, "max": 1 },
		"Ortho Factor": { "type": "float", "default": 2, "min": 1, "max": 3 },
		"Ortho Clockwise": { "type": "float", "default": 1, "min": -1, "max": 1 }
	},
	"Turbulence Curl": {
		"Curl Intensity": { "type": "float", "default": 0, "min": 0, "max": 1 },
		"Curl Frequency": { "type": "float", "default": 0, "min": 0, "max": 5 },
		"Curl Drag": { "type": "float", "default": 0, "min": 0, "max": 1 },
		"Curl Octaves": { "type": "float", "default": 1, "min": 1, "max": 8 },
		"Curl Roughness": { "type": "float", "default": 0, "min": 0, "max": 1 },
		"Curl Lacunarity": { "type": "float", "default": 0, "min": 0, "max": 1 },
		"Curl Scale": { "type": "float", "default": 0, "min": 0, "max": 1 },
		"Curl Translation": { "type": "float", "default": 0, "min": 0, "max": 1 }
	},
	"Perlin": {
		"Perlin Intensity": { "type": "float", "default": 0, "min": 0, "max": 1 },
		"Perlin Frequency": { "type": "float", "default": 0, "min": 0, "max": 5 },
		"Perlin Octaves": { "type": "float", "default": 1, "min": 1, "max": 8 },
		"Perlin Roughness": { "type": "float", "default": 0, "min": 0, "max": 1 },
		"Perlin Lacunarity": { "type": "float", "default": 0, "min": 0, "max": 1 },
		"Perlin Translation Speed": { "type": "float", "default": 0, "min": 0, "max": 1 }
	},
	"Orthoaxial": {
		"Orthoaxial Intensity": { "type": "float", "default": 0, "min": 0, "max": 1 },
		"Orthoaxial Inner Radius": { "type": "float", "default": 0, "min": 0, "max": 1 },
		"Orthoaxial Factor": { "type": "float", "default": 1, "min": 1, "max": 3 },
		"Orthoaxial Clockwise": { "type": "float", "default": 0, "min": 0, "max": 1 }
	}
};


var orbGroupParameters = {
	"General": {
		"Life": { "type": "float", "default": 20, "min": 0, "max": 40 },
		"Emitter Shape": { "type": "enum", "default": "Sphere", "values": ["Sphere", "Plane", "Torus", "Cube", "Pipe", "Egg", "Line", "Circle", "Merkaba", "Pyramid", "Custom"] },
		"Emitter Surface Factor": { "type": "float", "default": 0, "min": 0, "max": 1 },
		"Emitter Volume Factor": { "type": "float", "default": 0, "min": 0, "max": 1 },
	},
	"Appearance": {
		"Color": { "type": "color", "default": [.8, 2, .05] },
		"Alpha": { "type": "float", "default": 0.2, "min": 0, "max": 1 },
		"HDR Multiplier": { "type": "float", "default": 0, "min": 0, "max": 1 },
		"Alpha Speed Threshold": { "type": "float", "default": 0, "min": 0, "max": 1 },
		"Texture Opacity": { "type": "float", "default": 0, "min": 0, "max": 1 },
		"Particle Size": { "type": "float", "default": 0, "min": 0, "max": 1 },
	},
	"Physics": {
		"Force Weight": { "type": "float", "default": 1, "min": 0, "max": 1 },
		"Drag": { "type": "float", "default": 0.5, "min": 0, "max": 1 },
		"Velocity Drag": { "type": "float", "default": 0, "min": 0, "max": 1 },
		"Noisy Drag": { "type": "float", "default": 0, "min": 0, "max": 1 },
		"Noisy Drag Frequency": { "type": "float", "default": 0, "min": 0, "max": 1 },
		"Activate Collision": { "type": "bool", "default": false }
	}
};


//CALLBACKS
function init() {


	numForceGroupsParam = local.parameters.setup.numForceGroups;
	numOrbGroupsParam = local.parameters.setup.numOrbGroups;
	numMacrosParam = local.parameters.setup.numMacros;

	macrosGroup = local.parameters.macros;
	forceGroupsGroup = local.parameters.forceGroups;
	orbGroupsGroup = local.parameters.orbGroups;

	paramPropLinks = {};
	macroActiveParams = [];
	linkUnity();
	setup();
}


function moduleParameterChanged(param) {
	if (param.is(local.parameters.syncData)) {
		lastSyncTime = util.getTime();
	} else if (param.is(local.parameters.isConnected)) {
		if (local.parameters.isConnected.get()) {
			if (util.getTime() > lastSyncTime + 2) {
				local.parameters.syncData.trigger();
			}
		}
	} else if (param.is(numForceGroupsParam)) {
		if (unityForceGroupsParam) unityForceGroupsParam.set(numForceGroupsParam.get());
		setupForces();
		linkArrays();
	} else if (param.is(numOrbGroupsParam)) {
		if (unityOrbGroupsParam) unityOrbGroupsParam.set(numOrbGroupsParam.get());
		setupOrbs();
		linkArrays();
	} else if (param.is(numMacrosParam)) {
		setupMacros();
	} else if (param.name == "baseValue") {
		updateParam(param.getParent());
	} else if (param.name.startsWith("macroWeight")) {
		var index = parseInt(param.name.substring(11)) - 1;
		var paramItem = param.getParent();

		if (macroActiveParams[index] == null) macroActiveParams[index] = {};

		if (param.get() == 0) {
			//remove
			delete macroActiveParams[index][paramItem._ptr];
		} else {
			macroActiveParams[index][paramItem._ptr] = paramItem;
		}


		if (macros[index].get() > 0) updateParam(paramItem);


	} else if (param.getParent().is(macrosGroup)) {
		updateAllParametersForMacro(param);
	}
}


function dataStructureEvent() {
	linkUnity();
}


//UPDATE
function updateAllParametersForMacro(macroParam) {
	var index = parseInt(macroParam.name.substring(5)) - 1;
	for (var i in macroActiveParams[index]) {
		updateParam(macroActiveParams[index][i]);
	}
}


function updateParam(paramItem) {

	var valueParam = paramItem.baseValue;
	var paramRange = valueParam.getRange();

	var finalValue = valueParam.get();

	if (paramRange.length == 2) {
		for (var i = 0; i < macros.length; i++) {
			var macroWeightP = paramItem["macroWeight" + (i + 1)];
			if (macroWeightP == null) break;
			var macroWeight = macroWeightP.get();
			var macroValue = macros[i].get();
			var macroInfluence = macroValue * macroWeight * (paramRange[1] - paramRange[0]);
			finalValue += macroInfluence;
		}
		finalValue = Math.min(paramRange[1], Math.max(paramRange[0], finalValue));
	}

	var paramLink =	paramPropLinks[paramItem._ptr];
	var paramProp = paramLink.prop;
	var isForce = paramLink.isForce;


	var managerName = isForce ? forceGroupsGroup.name : orbGroupsGroup.name;

	var paramPath = "";
	if (paramProp.customComponent != null) {
		// script.log("Using custom component : " + paramProp.customComponent);
		paramPath = paramProp.customComponent;
	} else {
		var unityComponentName = isForce ? "StandardForceGroup" : "OrbGroup";
		paramPath = unityComponentName + "/" + paramItem.name;
	}


	updateUnityParam(managerName, paramItem.niceName, paramPath, finalValue);
}

function updateUnityParam(managerName, itemName, paramPath, value) {
	// script.log("Updating unity param " + managerName + "/" + itemName + "/" + paramPath + " to " + value);
	if (unityBallet == null) return;

	var param = unityBallet[managerName][itemName][paramPath];

	if (param == null) {
		script.logWarning("could not find unity param " + managerName + "." + itemName + "." + paramPath);
		return;
	}
	if (value != null) param.set(value);
}

// SETUP

function setup() {
	setupMacros();
}

function clearItems(group) {
	group.clear();
}

function linkArrays() {
	macros = macrosGroup.getControllables();
	forces = forceGroupsGroup.getContainers();
	orbGroups = orbGroupsGroup.getContainers();
}

function linkUnity() {
	unityBallet = local.values["ballet"];
	if (unityBallet == null) {
		script.log("No ballet found");
		unityOrbs = null;
		unityForces = null;
		unityOrbGroupsParam = null;
		unityForceGroupsParam = null;
		return;
	}

	unityOrbs = unityBallet[orbGroupsGroup.niceName];
	unityForces = unityBallet[forceGroupsGroup.niceName];

	unityOrbGroupsParam = unityOrbs.OrbManager.count;
	unityForceGroupsParam = unityForces.StandardForceManager.count;

	if (unityOrbGroupsParam) unityOrbGroupsParam.set(numOrbGroupsParam.get());
	if (unityForceGroupsParam) unityForceGroupsParam.set(numForceGroupsParam.get());
}

function setupMacros() {

	var macrosChanged = false;
	// if (macrosGroup.getControllables().length == numMacrosParam.get()) return;

	while (macrosGroup.getControllables().length > numMacrosParam.get()) {
		macrosGroup.removeParameter("macro" + (macrosGroup.getControllables().length));
		macrosChanged = true;
	}

	while (macrosGroup.getControllables().length < numMacrosParam.get()) {
		var macro = macrosGroup.addFloatParameter("Macro " + (macrosGroup.getControllables().length + 1), "Macro value", 0, 0, 1);
		macro.setAttribute("saveValueOnly", false);
		macrosChanged = true;
	}

	macrosGroup = local.parameters.macros;
	macros = macrosGroup.getControllables();

	setupForces(macrosChanged);
	setupOrbs(macrosChanged);

	linkArrays();
}

function setupForces(updateMacros) {
	if (numForceGroupsParam == null) return;
	setupParameters(forceGroupsGroup, numForceGroupsParam, forceGroupParameters, forces, "Force Group", updateMacros);
}

function setupOrbs(updateMacros) {
	if (numOrbGroupsParam == null) return;
	setupParameters(orbGroupsGroup, numOrbGroupsParam, orbGroupParameters, orbGroups, "Orb Group", updateMacros);
}

function setupParameters(group, numParam, parameters, items, prefix, updateMacros) {

	var numItems = numParam.get();

	var existingItems = group.getContainers();
	var numExistingItems = existingItems ? existingItems.length : 0;

	while (numExistingItems > numItems) {
		group.removeContainer(existingItems[numExistingItems - 1].name);
		numExistingItems--;
		items.splice(items.length - 1);
	}

	// if (updateMacros) {
	for (var i = 0; i < Math.min(numExistingItems, numItems); i++) {
		setupMacrosToItem(existingItems[i], parameters);
	}
	// }

	for (var i = numExistingItems; i < numItems; i++) {
		createItem(i, group, parameters, prefix);
	}

	items = group.getContainers();
}

function createItem(index, group, parameters, prefix) {
	var item = group.addContainer(prefix + " " + (index + 1));
	item.setCollapsed(true);
	addParametersToItem(item, danceGroupParameters);
	addParametersToItem(item, parameters);

	script.refreshEnvironment();

	item = group.addContainer(prefix + " " + (index + 1));
	setupMacrosToItem(item, parameters);

	return item;
}

function addParametersToItem(item, parameters) {
	for (var groupName in parameters) {
		var groupParams = parameters[groupName];
		var paramGroup = item.addContainer(groupName);

		for (var paramName in groupParams) {
			var paramConfig = groupParams[paramName];
			var paramType = paramConfig.type;
			var paramDefault = paramConfig.default;
			var paramMin = paramConfig.min;
			var paramMax = paramConfig.max;

			var paramContainer = paramGroup.addContainer(paramName);

			var param = null;
			if (paramType == "float") {
				if (paramMin != null && paramMax != null) param = paramContainer.addFloatParameter("Base Value", "Base value for this parameter", paramDefault, paramMin, paramMax);
				else if (paramMin != null) param = paramContainer.addFloatParameter("Base Value", "Base value for this parameter", paramDefault, paramMin);
				else param = paramContainer.addFloatParameter("Base Value", "Base value for this parameter", paramDefault);
			} else if (paramType == "int") {
				if (paramMin != null && paramMax != null) param = paramContainer.addIntParameter("Base Value", "Base value for this parameter", paramDefault, paramMin, paramMax);
				else if (paramMin != null) param = paramContainer.addIntParameter("Base Value", "Base value for this parameter", paramDefault, paramMin);
				else param = paramContainer.addIntParameter("Base Value", "Base value for this parameter", paramDefault);
			} else if (paramType == "p3d") {
				param = paramContainer.addPoint3DParameter("Base Value", "Base value for this parameter", paramDefault);
			} else if (paramType == "color") {
				param = paramContainer.addColorParameter("Base Value", "Base value for this parameter", paramDefault);
			} else if (paramType == "enum") {
				var ep = param = paramContainer.addEnumParameter("Base Value", "Base value for this parameter", paramDefault);
				for (var v = 0; v < paramConfig.values.length; v++) {
					ep.addOption(paramConfig.values[v], paramConfig.values[v]);
				}
			} else if (paramType == "bool") {
				param = paramContainer.addBoolParameter("Base Value", "Base value for this parameter", paramDefault);
			}

			if (param != null) {
				param.setAttribute("saveValueOnly", false);
			}
		}
	}
}

function getShortName(name) {
	return name.split(" ").map((s, i) => i == 0 ? s.toLowerCase() : s.charAt(0).toUpperCase() + s.slice(1)).join("");
}

function setupMacrosToItem(item, parameters) {

	var isForce = parameters == forceGroupParameters;

	var allParams = { ...danceGroupParameters, ...parameters };

	var numMacros = numMacrosParam.get();

	for (var groupName in allParams) {
		var groupShortName = getShortName(groupName);
		var itemGroup = item[groupShortName];

		// script.log(itemGroup);
		for (var paramName in allParams[groupName]) {
			var paramProp = allParams[groupName][paramName];

			var paramShortName = getShortName(paramName);
			var paramContainer = itemGroup[paramShortName];

			var paramMacro = numMacros;
			var isEligible = !paramProp.noMacro && (paramProp.type == "float" || paramProp.type == "int") && (paramProp.min != null && paramProp.min != null);
			if (!isEligible) paramMacro = 0;


			paramPropLinks[paramContainer._ptr] = {prop:paramProp,isForce:isForce};

			// for (var k = paramCurrentMacros; k > numMacros; k--) paramContainer.removeParameter("Macro Weight " + k);

			for (var k = 0; k < paramMacro; k++) {

				var mp = paramContainer["macroWeight" + (k + 1)];
				if (mp == null) {
					mp = paramContainer.addFloatParameter("Macro Weight " + (k + 1), "Macro influence for this parameter, relative to the parameters range if it has any", 0, -1, 1);
					mp.setAttribute("saveValueOnly", false);
				}

				if (mp.get() != 0) {
					if (macroActiveParams[k] == null) macroActiveParams[k] = {};
					macroActiveParams[k][mp._ptr] = paramContainer;
				}
			}
		}
	}
}