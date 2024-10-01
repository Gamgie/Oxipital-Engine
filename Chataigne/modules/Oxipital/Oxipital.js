var numForcesParam = local.parameters.setup.numForces;
var numOrbGroupsParam = local.parameters.setup.numOrbGroups;
var numMacrosParam = local.parameters.setup.numMacros;

var macrosGroup = local.parameters.macros;
var forcesGroup = local.parameters.forces;
var orbGroupsGroup = local.parameters.orbGroups;

var forces = [];
var orbGroups = [];
var macros = [];

//Unity links
var unityForcesGroup = local.values.orbs.forces;

var forceParameters = {
	"General":
	{
		"Position": { "type": "p3d", "default": [0, 0, 0] },
		"Radius": { "type": "float", "default": 0, "min": 0, "max": 15 },
		"Intensity": { "type": "float", "default": 0, "min": 0, "max": 100 },
	},
	"Turbulence":
	{
		"Intensity": { "type": "float", "default": 0, "min": 0, "max": 1 },
		"Frequency": { "type": "float", "default": 0, "min": 0 },
		"Octaves": { "type": "int", "default": 0, "min": 0, "max": 6 },
	},
	"Vortex":
	{
		"Intensity": { "type": "float", "default": 0, "min": 0, "max": 1 },
		"Frequency": { "type": "float", "default": 0, "min": 0, "max": 20 },
	},
	"Gravity":
	{
		"Intensity": { "type": "float", "default": 0, "min": 0, "max": 1 },
		"Axis": { "type": "p3d", "default": [0, 1, 0] },
	}
};


var orbGroupParameters = {
	"General":
	{
		"Position": { "type": "p3d", "default": [0, 0, 0] },
		"Rotation": { "type": "p3d", "default": [0, 0, 0] },
		"Scale": { "type": "p3d", "default": [1, 1, 1] },
		"Size": { "type": "float", "default": 0, "min": 0, "max": 1 },
		"Size Offset": { "type": "float", "default": 0, "min": 0, "max": 1 },
		"Life": { "type": "float", "default": 0, "min": 0, "max": 200 },
	},
	"Emission": {
		"Rate": { "type": "float", "default": 0, "min": 0, "max": 1 },
		"Shape": { "type": "enum", "default": "Sphere", "values": ["Sphere", "Plane", "Torus", "Cube", "Pipe", "Egg", "Line", "Circle", "Merkaba", "Pyramid", "Landscape"] },
		"Mode": { "type": "enum", "default": "Surface", "values": ["Surface", "Edge"] },
		"Volumize": { "type": "float", "default": 0, "min": 0, "max": 1 },
	},
	"Appearance": {
		"Size": { "type": "float", "default": 0, "min": 0, "max": 1 },
		"Alpha": { "type": "float", "default": 0, "min": 0, "max": 1 },
		"Color": { "type": "color", "default": [1, 1, 1] },
		"Color Multiplier": { "type": "float", "default": 0, "min": 0, "max": 5 },
		"Stationary": { "type": "float", "default": 0, "min": 0, "max": 1 }
	},
	"Physics": {
		"Forces Multiplier": { "type": "float", "default": 0, "min": 0, "max": 1 },
		"Drag": { "type": "float", "default": 0, "min": 0, "max": 1 },
		"Velocity Drag": { "type": "float", "default": 0, "min": 0, "max": 1 },
		"Noisy Drag": { "type": "float", "default": 0, "min": 0, "max": 1 },
		"Noisy Drag Frequency": { "type": "float", "default": 0, "min": 0, "max": 1 }
	}
};

//CALLBACKS
function init() {
	setupMacros();
	setupForceParameters(false);
	setupOrbParameters(false);

	linkArrays();
	script.log("Forces", forces.length);
}


function moduleParameterChanged(param) {
	if (param.is(numForcesParam)) {
		setupForceParameters();
	} else if (param.is(numOrbGroupsParam)) {
		setupOrbParameters();
	} else if (param.is(numMacrosParam)) {
		setupMacros();
	} else if (param.getParent().is(macrosGroup)) {
		updateAllParametersForMacro(param);
	} else {
		//To optimize , create in Chataigne an optional parameter level in getParent() to avoid checking every step
		var p4 = param.getParent(4);
		if (p4 == forcesGroup) {
			var forceIndex = parseInt(param.getParent(3).niceName.split(" ")[1]) - 1;
			var groupName = param.getParent(2).niceName;
			var paramName = param.getParent().niceName;
			updateForceParam(forceIndex, groupName, paramName, param);
		} else if (p4 == orbGroupsGroup) {
			var orbGroupIndex = parseInt(param.getParent(3).niceName.split(" ")[2]) - 1;
			var groupName = param.getParent(2).niceName;
			var paramName = param.getParent().niceName;
			updateOrbGroupParam(orbGroupIndex, groupName, paramName, param);
		}
	}
}

// function moduleValueChanged(value) 
// {
// }


//UPDATE

function updateForceParam(forceIndex, groupName, paramName, sourceParam) {

	//check if sourceParam is one of the 4 macros, and stop if it is and it's related macro has a value of 0
	if (sourceParam != null) {
		var index = sourceParam.getParent().getControllables().indexOf(sourceParam);
		if (index > 0 && macros[index - 1].get() == 0) {
			return;
		}
	}

	var force = forces[forceIndex];
	var forceGroup = force.getChild(groupName);
	var forceParamGroup = forceGroup.getChild(paramName);
	var forceParam = forceParamGroup.getChild("baseValue");
	var paramMin = forceParameters[groupName][paramName].min;
	var paramMax = forceParameters[groupName][paramName].max;

	var finalValue = forceParam.get();

	if (paramMin != null && paramMax != null) {
		for (var i = 0; i < numMacrosParam.get(); i++) {
			var macroWeight = forceParamGroup.getChild("macroWeight" + (i + 1)).get();
			var macroValue = macros[i].get();
			var macroInfluence = macroValue * macroWeight * (paramMax - paramMin);
			finalValue += macroInfluence;
		}
	}

	finalValue = Math.min(paramMax, Math.max(paramMin, finalValue));

	script.log("Update Force Param " + (forceIndex + 1) + " " + groupName + " " + paramName + " " + finalValue);
}

function updateOrbGroupParam(orbGroupIndex, groupName, paramName, sourceParam) {

	if (sourceParam != null) {
		var index = sourceParam.getParent().getControllables().indexOf(sourceParam);
		if (index > 0 && macros[index - 1].get() == 0) {
			return;
		}
	}
	
	var orbGroup = orbGroups[orbGroupIndex];
	var orbGroupGroup = orbGroup.getChild(groupName);
	var orbGroupParamGroup = orbGroupGroup.getChild(paramName);
	var orbGroupParam = orbGroupParamGroup.getChild("baseValue");
	var paramMin = orbGroupParameters[groupName][paramName].min;
	var paramMax = orbGroupParameters[groupName][paramName].max;

	var finalValue = orbGroupParam.get();

	if (paramMin != null && paramMax != null) {
		for (var i = 0; i < numMacrosParam.get(); i++) {
			var macroWeight = orbGroupParamGroup.getChild("macroWeight" + (i + 1)).get();
			var macroValue = macros[i].get();
			var macroInfluence = macroValue * macroWeight * (paramMax - paramMin);
			finalValue += macroInfluence;
		}
	}

	finalValue = Math.min(paramMax, Math.max(paramMin, finalValue));

	script.log("Update Orb Group Param " + (orbGroupIndex + 1) + " " + groupName + " " + paramName + " " + finalValue);
	// unityOrbGroupsGroup.getChild("Orb Group " + (orbGroupIndex + 1)).getChild(groupName).getChild(paramName).set(finalValue);
}

function updateAllParametersForMacro(macroParam) {
	var index = macrosGroup.getControllables().indexOf(macroParam);

	for (var i = 0; i < forces.length; i++) {
		var force = forces[i];
		var forceGroups = force.getContainers();
		for (var j = 0; j < forceGroups.length; j++) {
			var forceGroup = forceGroups[j];
			var forceParams = forceGroup.getContainers();
			for (var k = 0; k < forceParams.length; k++) {
				var forceParamGroup = forceParams[k];

				var forceParamChildren = forceParamGroup.getControllables();
				if(forceParamChildren.length <= index +1) continue
				var macroParam = forceParamChildren[index+1];
				if(macroParam.get() == 0) continue;
				updateForceParam(i, forceGroup.niceName, forceParamGroup.niceName, macroParam);
			}
		}
	}

	for (var i = 0; i < orbGroups.length; i++) {
		var orbGroup = orbGroups[i];
		var orbGroupGroups = orbGroup.getContainers();
		for (var j = 0; j < orbGroupGroups.length; j++) {
			var orbGroupGroup = orbGroupGroups[j];
			var orbGroupParams = orbGroupGroup.getContainers();
			for (var k = 0; k < orbGroupParams.length; k++) {
				var orbGroupParamGroup = orbGroupParams[k];

				var orbGroupParamChildren = orbGroupParamGroup.getControllables();
				if (orbGroupParamChildren.length <= index + 1) continue;
				var macroParam = orbGroupParamChildren[index + 1];
				if (macroParam.get() == 0) continue;
				updateOrbGroupParam(i, orbGroupGroup.niceName, orbGroupParamGroup.niceName, macroParam);
			}
		}
	}

}

// SETUP

function clearForces() {
	forcesGroup.clear();
	forces = [];
}

function linkArrays() {
	macros = macrosGroup.getControllables();
	forces = forcesGroup.getContainers();
	orbGroups = orbGroupsGroup.getContainers();
}

function setupMacros() {

	if (macrosGroup.getControllables().length == numMacrosParam.get()) return;

	while (macrosGroup.getControllables().length > numMacrosParam.get()) {
		macrosGroup.removeParameter("macro" + (macrosGroup.getControllables().length));
	}

	while (macrosGroup.getControllables().length < numMacrosParam.get()) {
		var macro = macrosGroup.addFloatParameter("Macro " + (macrosGroup.getControllables().length + 1), "Macro value", 0, 0, 1);
	}

	macros = macrosGroup.getControllables();
	setupForceParameters(true);
	setupOrbParameters(true);
}

function setupForceParameters(clear) {

	if (clear) clearForces();

	var numForces = numForcesParam.get();

	var existingForces = forcesGroup.getContainers();
	var numExistingForces = existingForces ? existingForces.length : 0;

	while (numExistingForces > numForces) {
		forcesGroup.removeContainer(existingForces[numExistingForces - 1].name);
		numExistingForces--;
		forces.splice(forces.length - 1);
	}

	for (var i = numExistingForces; i < numForces; i++) {
		var force = createForce(i);
	}

	forces = forcesGroup.getContainers();
}

function createForce(index) {
	var force = forcesGroup.addContainer("Force " + (index + 1));
	force.setCollapsed(true);
	var forceParamGroupProps = util.getObjectProperties(forceParameters);

	for (var i = 0; i < forceParamGroupProps.length; i++) {
		var groupName = forceParamGroupProps[i];
		var groupParams = forceParameters[groupName];

		var group = force.addContainer(groupName);
		var groupParamsProps = util.getObjectProperties(groupParams);
		for (var j = 0; j < groupParamsProps.length; j++) {
			var paramName = groupParamsProps[j];
			var paramConfig = groupParams[paramName];
			var paramType = paramConfig.type;
			var paramDefault = paramConfig.default;
			var paramMin = paramConfig.min;
			var paramMax = paramConfig.max;


			var paramContainer = group.addContainer(paramName);

			if (paramType == "float") {
				if (paramMin != null && paramMax != null) paramContainer.addFloatParameter("Base Value", "Base value for this parameter", paramDefault, paramMin, paramMax);
				else if (paramMin != null) paramContainer.addFloatParameter("Base Value", "Base value for this parameter", paramDefault, paramMin);
				else paramContainer.addFloatParameter("Base Value", "Base value for this parameter", paramDefault);
			} else if (paramType == "int") {
				if (paramMin != null && paramMax != null) paramContainer.addIntParameter("Base Value", "Base value for this parameter", paramDefault, paramMin, paramMax);
				else if (paramMin != null) paramContainer.addIntParameter("Base Value", "Base value for this parameter", paramDefault, paramMin);
				else paramContainer.addIntParameter("Base Value", "Base value for this parameter", paramDefault);
			} else if (paramType == "p3d") {
				paramContainer.addPoint3DParameter("Base Value", "Base value for this parameter", paramDefault);
			} else if (paramType == "color") {
				paramContainer.addColorParameter("Base Value", "Base value for this parameter", paramDefault);
			}

			if (paramType == "float" || paramType == "int" && (paramMin != null && paramMax != null)) {
				for (var k = 0; k < numMacrosParam.get(); k++) {
					paramContainer.addFloatParameter("Macro Weight " + (k + 1), "Macro influence for this parameter, relative to the parameters range if it has any", 0, -1, 1);
				}

			}

		}
	}

	return force;

}


function setupOrbParameters(clear) {
	if (clear) clearOrbGroups();

	var numOrbGroups = numOrbGroupsParam.get();

	var existingOrbGroups = orbGroupsGroup.getContainers();
	var numExistingOrbGroups = existingOrbGroups ? existingOrbGroups.length : 0;

	while (numExistingOrbGroups > numOrbGroups) {
		orbGroupsGroup.removeContainer(existingOrbGroups[numExistingOrbGroups - 1].name);
		numExistingOrbGroups--;
		orbGroups.splice(orbGroups.length - 1);
	}

	for (var i = numExistingOrbGroups; i < numOrbGroups; i++) {
		var orbGroup = createOrbGroup(i);
	}

	orbGroups = orbGroupsGroup.getContainers();
}

function clearOrbGroups() {
	orbGroupsGroup.clear();
}

function createOrbGroup(index) {
	var orbGroup = orbGroupsGroup.addContainer("Orb Group " + (index + 1));
	orbGroup.setCollapsed(true);
	var orbGroupParamGroupProps = util.getObjectProperties(orbGroupParameters);

	for (var i = 0; i < orbGroupParamGroupProps.length; i++) {
		var groupName = orbGroupParamGroupProps[i];
		var groupParams = orbGroupParameters[groupName];

		var group = orbGroup.addContainer(groupName);
		var groupParamsProps = util.getObjectProperties(groupParams);
		for (var j = 0; j < groupParamsProps.length; j++) {
			var paramName = groupParamsProps[j];
			var paramConfig = groupParams[paramName];
			var paramType = paramConfig.type;
			var paramDefault = paramConfig.default;
			var paramMin = paramConfig.min;
			var paramMax = paramConfig.max;

			var paramContainer = group.addContainer(paramName);

			if (paramType == "float") {
				if (paramMin != null && paramMax != null) paramContainer.addFloatParameter("Base Value", "Base value for this parameter", paramDefault, paramMin, paramMax);
				else if (paramMin != null) paramContainer.addFloatParameter("Base Value", "Base value for this parameter", paramDefault, paramMin);
				else paramContainer.addFloatParameter("Base Value", "Base value for this parameter", paramDefault);
			} else if (paramType == "int") {
				if (paramMin != null && paramMax != null) paramContainer.addIntParameter("Base Value", "Base value for this parameter", paramDefault, paramMin, paramMax);
				else if (paramMin != null) paramContainer.addIntParameter("Base Value", "Base value for this parameter", paramDefault, paramMin);
				else paramContainer.addIntParameter("Base Value", "Base value for this parameter", paramDefault);
			} else if (paramType == "p3d") {
				paramContainer.addPoint3DParameter("Base Value", "Base value for this parameter", paramDefault);
			} else if (paramType == "color") {
				paramContainer.addColorParameter("Base Value", "Base value for this parameter", paramDefault);
			} else if (paramType == "enum") {
				paramContainer.addEnumParameter("Base Value", "Base value for this parameter", paramDefault, paramConfig.values);
			}

			if (paramType == "float" || paramType == "int" && (paramMin != null && paramMax != null)) {
				for (var k = 0; k < numMacrosParam.get(); k++) {
					paramContainer.addFloatParameter("Macro Weight " + (k + 1), "Macro influence for this parameter, relative to the parameters range if it has any", 0, -1, 1);
				}
			}
		}
	}

	return orbGroup;

	return orbGroup;
}
