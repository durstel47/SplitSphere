using System;
using UnityEngine;

namespace UnityStandardAssets.ImageEffects
{
	[ExecuteInEditMode]
	[AddComponentMenu("Image Effects/SecondSplitSphereContrastTexture")]
	[RequireComponent (typeof(Camera))]
	//right and right eye need different shaders, therefore the need for the SecondSplit
	public class SecondSplitSphereContrastTexture : ImageEffectBase
	{
		/********************************
		public Texture mainTexture {
			get { return _mainTexture;  }
			set { _mainTexture = value; Debug.Log ("Got main texture: " + _mainTexture.name);} }
		********/
		public Texture maskTexture {
			get { return _maskTexture; }
			set { _maskTexture = value; } }
		public Vector2 mainTextureOffset{
			get { return _mainTextureOffset; }
			set { _mainTextureOffset = value; } }
		public Vector2 mainTextureScale {
			get { return _mainTextureScale; }
			set { _mainTextureScale = value; } }
		public float pheripheralContrast {
			get { return _pheripheralContrast; }
			set { _pheripheralContrast = value; } }
		public float isMirroredPeriphery {
			get { return _isMirroredPeriphery; }
			set { _isMirroredPeriphery = value; } }
		public float centralContrast {
			get { return _centralContrast; }
			set { _centralContrast = value;} }
		public float isMirroredCenter {
			get { return _isMirroredCenter; }
			set { _isMirroredCenter = value; } }
		public Vector2 maskTexOffset {
			get { return _maskTexOffset; }
			set { _maskTexOffset = value; } }
		public Vector2 maskTexScale {
			get { return _maskTexScale; }
			set { _maskTexScale = value; } }
		public float contrast {
			get { return _contrast; }
			set { _contrast = value; } }
		public float maskMidPointX {
			get { return _maskMidPointX; }
			set { _maskMidPointX = value; } }
		public float maskScaleX {
			get { return _maskScaleX; }
			set { _maskScaleX = value; } }
		public int isGray {
			get { return _isGray; }
			set { _isGray = value; } }


		//private Texture _mainTexture;
		private Texture _maskTexture;
		private Vector2 _mainTextureOffset;
		private Vector2 _mainTextureScale;
		private float _pheripheralContrast = 1f;
		private float _isMirroredPeriphery = 0f;
		private float _centralContrast = 1f;
		private float _isMirroredCenter = 0f;
		private Vector2 _maskTexOffset;
		private Vector2 _maskTexScale;
		private float _contrast = 1f;
		private float _maskMidPointX = 0f;
		private float _maskScaleX = 1f;
		private int _isGray = 0;


		override protected void Start()
		{
			_mainTextureOffset = new Vector2 (0f, 0f);
			_mainTextureScale = new Vector2 (1f, 1f);
			_maskTexOffset = new Vector2 (0.25f, 0f);
			_maskTexScale = new Vector2 (0.5f, 1f);
			base.Start ();
		}


		// Called by camera to apply image effect
		void OnRenderImage (RenderTexture source, RenderTexture destination) {
						
			Graphics.Blit (source, destination, material);

			material.SetTextureOffset("_MainTex", _mainTextureOffset);
			material.SetTextureScale ("_MainTex", _mainTextureScale);
			material.SetTexture("_MaskTex", _maskTexture);
			material.SetTextureOffset("_MaskTex", _maskTexOffset);
			material.SetTextureScale ("_MaskTex", _maskTexScale);
			material.SetFloat ("_PeripheralContrast", _pheripheralContrast);
			material.SetFloat ("_IsMirroredPeriphery", _isMirroredPeriphery);
			material.SetFloat ("_CentralContrast", _centralContrast);
			material.SetFloat ("_IsMirroredCenter", _isMirroredCenter);
			material.SetFloat ("_Contrast", _contrast);
			material.SetFloat ("_MaskMidPointX", _maskMidPointX);
			material.SetFloat ("_MaskScaleX", _maskScaleX);
			material.SetInt ("_IsGray", _isGray);

		}

	}
}
