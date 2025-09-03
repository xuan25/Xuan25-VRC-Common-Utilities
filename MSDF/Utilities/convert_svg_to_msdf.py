"""
SVG Batch Processor for msdfgen

This module provides functionality to batch process SVG files using msdfgen (multi-channel signed distance field generator).
It includes preprocessing capabilities using Inkscape to convert strokes to paths, merge multiple paths,
and scale SVGs before processing.

Features:
- Batch process SVG files in folders and subfolders
- Maintain folder structure in output
- Convert strokes to paths using Inkscape
- Merge multiple paths into single shapes
- Scale SVGs before processing
- Support for multiple msdfgen modes (sdf, psdf, msdf, mtsdf)
- Debug support with preprocessed file preservation

Requires:
- Inkscape (command line interface): https://inkscape.org/
- msdfgen (command line interface): https://github.com/Chlumsky/msdfgen
"""

import os
import re
import subprocess
import argparse
import logging
import tempfile
import shutil
from pathlib import Path
from typing import Optional, Tuple, List, Union
import xml.etree.ElementTree as ET

class SVGProcessingError(Exception):
    """Custom exception for SVG processing errors."""
    pass


class InkscapeError(Exception):
    """Custom exception for Inkscape-related errors."""
    pass


class MSDFGenError(Exception):
    """Custom exception for msdfgen-related errors."""
    pass


class SVGProcessor:
    """
    A class to handle SVG preprocessing operations.
    
    This class encapsulates all SVG preprocessing logic including stroke-to-path conversion,
    path merging, and scaling operations using both XML manipulation and Inkscape.
    """
    
    # Class constants
    SVG_NAMESPACE = 'http://www.w3.org/2000/svg'
    METADATA_TAGS = {'metadata', 'defs', 'title', 'desc'}
    DEFAULT_DIMENSIONS = (100.0, 100.0)
    
    def __init__(self, logger: Optional[logging.Logger] = None):
        """
        Initialize the SVG processor.
        
        Args:
            logger: Optional logger instance for logging operations
        """
        self.logger = logger or logging.getLogger(__name__)
    
    def preprocess_svg(self, 
                      svg_file_path: Path, 
                      merge_paths: bool = True, 
                      scale_size: Optional[Tuple[int, int]] = None) -> str:
        """
        Preprocess SVG file to convert strokes to paths and optionally merge paths.
        
        This method always performs stroke-to-path conversion and optionally merges paths
        based on the merge_paths parameter.
        
        Args:
            svg_file_path: Path to the input SVG file
            merge_paths: Whether to merge multiple paths into one (additional step)
            scale_size: Target size (width, height) to scale the SVG to, or None for no scaling
        
        Returns:
            Path to the preprocessed SVG file (temporary file if processed, original if not)
            
        Raises:
            SVGProcessingError: If SVG processing fails
        """
        try:
            processed_path = self._process_svg_pipeline(svg_file_path, merge_paths, scale_size)
            return processed_path
            
        except (ET.ParseError, OSError, ValueError, AttributeError) as e:
            error_msg = f"Could not preprocess SVG {svg_file_path}: {str(e)}"
            self.logger.warning(error_msg)
            self.logger.warning("Using original file...")
            return str(svg_file_path)
    
    def _process_svg_pipeline(self, 
                              svg_file_path: Path, 
                              merge_paths: bool = True, 
                              scale_size: Optional[Tuple[int, int]] = None) -> str:
        """
        Execute the complete SVG processing pipeline: scaling, stroke-to-path, and optional merging.
        
        Args:
            svg_file_path: Path to the input SVG file
            merge_paths: Whether to merge multiple paths into one
            scale_size: Target size (width, height) to scale the SVG to, or None for no scaling
        
        Returns:
            Path to the processed SVG file (temporary file if processed, original if not)
            
        Raises:
            InkscapeError: If Inkscape processing fails
        """
        try:
            current_svg_path = svg_file_path
            scaled_svg_path = None
            
            # Step 1: Handle scaling via XML if requested
            if scale_size:
                target_width, target_height = scale_size
                scaled_svg_path = self._scale_svg_xml(current_svg_path, target_width, target_height)
                current_svg_path = Path(scaled_svg_path)
                self.logger.debug(f"Applied XML scaling to {target_width}x{target_height}")
            
            # Step 2: Handle path operations with Inkscape (always needed for stroke-to-path)
            inkscape_processed_path = None
            # Create a temporary file for Inkscape processing
            temp_fd, temp_path = tempfile.mkstemp(suffix='.svg', prefix='inkscape_processed_')
            os.close(temp_fd)
            
            # Build Inkscape actions for path operations
            actions = self._build_inkscape_actions(merge_paths)
            
            # Execute Inkscape processing on the current SVG (scaled or original)
            success = self._execute_inkscape(current_svg_path, temp_path, actions)
            
            if success:
                inkscape_processed_path = temp_path
                # Clean up the intermediate scaled file if it exists
                if scaled_svg_path and scaled_svg_path != str(svg_file_path):
                    self._cleanup_temp_file(scaled_svg_path)
                return inkscape_processed_path
            else:
                # Clean up failed temp file
                self._cleanup_temp_file(temp_path)
            
            # Return the scaled version if we have it, otherwise original
            if scaled_svg_path:
                return scaled_svg_path
            else:
                return str(svg_file_path)
                
        except Exception as e:
            self.logger.warning(f"Could not run SVG processing: {str(e)}")
            # Clean up any temp files on error
            if scaled_svg_path and scaled_svg_path != str(svg_file_path):
                self._cleanup_temp_file(scaled_svg_path)
            return str(svg_file_path)
    
    def _build_inkscape_actions(self, merge_paths: bool) -> List[str]:
        """
        Build list of Inkscape actions for stroke-to-path conversion and optional merging.
        
        Args:
            merge_paths: Whether to merge paths after stroke conversion
            
        Returns:
            List of Inkscape action strings
        """
        actions = []
        
        # Always select all at the beginning
        actions.append('select-all')
        
        # Always convert strokes to paths (essential step)
        actions.extend(['object-stroke-to-path', 'select-all'])
        
        # Merge paths if requested (additional step)
        if merge_paths:
            actions.append('path-union')
        
        return actions
    
    def _execute_inkscape(self, input_path: Path, output_path: str, actions: List[str]) -> bool:
        """
        Execute Inkscape with the specified actions.
        
        Args:
            input_path: Path to input SVG file
            output_path: Path to output SVG file
            actions: List of Inkscape actions to execute
            
        Returns:
            True if successful, False otherwise
        """
        if not actions:
            return False
        
        # Use action-based processing for all operations
        actions_string = ';'.join(actions)
        cmd = [
            'inkscape',
            str(input_path),
            f'--actions={actions_string}',
            '--export-type=svg',
            f'--export-filename={output_path}'
        ]
        
        try:
            result = subprocess.run(cmd, capture_output=True, text=True, check=False)
            
            if result.returncode == 0 and os.path.exists(output_path):
                # Verify the output file has content
                if os.path.getsize(output_path) > 0:
                    return True
                else:
                    self.logger.warning("Inkscape produced empty file, using original")
                    return False
            else:
                self.logger.warning("Inkscape processing failed, using original file")
                if result.stderr:
                    self.logger.warning(f"Inkscape error: {result.stderr.strip()}")
                return False
                
        except (subprocess.SubprocessError, OSError) as e:
            self.logger.warning(f"Failed to execute Inkscape: {e}")
            return False
    
    @staticmethod
    def _cleanup_temp_file(file_path: str) -> None:
        """
        Safely clean up a temporary file.
        
        Args:
            file_path: Path to the file to clean up
        """
        try:
            if os.path.exists(file_path):
                os.unlink(file_path)
        except OSError:
            pass

    def _parse_svg_dimensions(self, svg_file_path: Path) -> Tuple[float, float]:
        """
        Parse SVG file to extract width and height dimensions.
        
        Args:
            svg_file_path: Path to the SVG file
            
        Returns:
            Tuple of (width, height) in pixels
            
        Raises:
            ValueError: If dimensions cannot be parsed
        """
        try:
            tree = ET.parse(svg_file_path)
            root = tree.getroot()
            
            # Get width and height attributes
            width_attr = root.get('width', str(self.DEFAULT_DIMENSIONS[0]))
            height_attr = root.get('height', str(self.DEFAULT_DIMENSIONS[1]))
            
            width = self._parse_dimension(width_attr)
            height = self._parse_dimension(height_attr)
            
            # If no width/height attributes, try to parse viewBox
            if width == self.DEFAULT_DIMENSIONS[0] and height == self.DEFAULT_DIMENSIONS[1]:
                width, height = self._parse_viewbox_dimensions(root)
            
            # Ensure we have valid dimensions
            if width <= 0 or height <= 0:
                raise ValueError(f"Invalid dimensions: {width}x{height}")
            
            return width, height
            
        except (ET.ParseError, ValueError, AttributeError) as e:
            raise ValueError(f"Failed to parse SVG dimensions: {e}")
    
    def _parse_dimension(self, dim_str: str) -> float:
        """
        Parse a dimension string, removing units if present.
        
        Args:
            dim_str: Dimension string (e.g., "100px", "50", "2.5cm")
            
        Returns:
            Numeric value as float
        """
        # Remove units like px, pt, cm, mm, in, etc.
        number_match = re.search(r'[\d.]+', dim_str)
        if number_match:
            return float(number_match.group())
        return self.DEFAULT_DIMENSIONS[0]  # Default fallback
    
    def _parse_viewbox_dimensions(self, root: ET.Element) -> Tuple[float, float]:
        """
        Extract dimensions from viewBox attribute.
        
        Args:
            root: SVG root element
            
        Returns:
            Tuple of (width, height) from viewBox
        """
        viewbox = root.get('viewBox')
        if viewbox:
            parts = viewbox.split()
            if len(parts) == 4:
                return float(parts[2]), float(parts[3])
        return self.DEFAULT_DIMENSIONS

    def _scale_svg_xml(self, 
                       svg_file_path: Path, 
                       target_width: int, 
                       target_height: int) -> str:
        """
        Scale SVG by directly modifying the XML structure.
        
        This method parses the SVG XML, calculates the appropriate scaling factor,
        and modifies the SVG structure to scale content while preserving aspect ratio
        and centering the content within the target dimensions.
        
        Args:
            svg_file_path: Path to the input SVG file
            target_width: Target width for the scaled SVG
            target_height: Target height for the scaled SVG
            
        Returns:
            Path to the scaled SVG file (temporary file)
            
        Raises:
            SVGProcessingError: If XML scaling fails
        """
        try:
            # Parse the original SVG
            tree = ET.parse(svg_file_path)
            root = tree.getroot()
            
            # Get original dimensions
            original_width, original_height = self._parse_svg_dimensions(svg_file_path)
            
            # Calculate scale factor (proportional scaling)
            scale_x = target_width / original_width
            scale_y = target_height / original_height
            scale_factor = min(scale_x, scale_y)
            
            # Calculate scaled dimensions and centering offset
            scaled_width = original_width * scale_factor
            scaled_height = original_height * scale_factor
            offset_x = (target_width - scaled_width) / 2
            offset_y = (target_height - scaled_height) / 2
            
            # Update SVG root attributes
            root.set('width', str(target_width))
            root.set('height', str(target_height))
            
            # Ensure proper SVG namespace (only if not already present)
            self._ensure_svg_namespace(root)
            
            # Set viewBox to target dimensions for proper scaling
            root.set('viewBox', f'0 0 {target_width} {target_height}')
            
            # Find or create a group to contain all content
            content_group = self._get_or_create_content_group(root)
            
            # Apply transform to the content group for proper scaling within the new viewBox
            self._apply_scaling_transform(content_group, offset_x, offset_y, scale_factor)
            
            # Create temporary file for the scaled SVG
            temp_path = self._create_temp_svg_file()
            
            # Write the modified SVG with proper formatting
            self._write_svg_file(tree, temp_path)
            
            # Validate the output file
            self._validate_svg_output(temp_path)
            
            self.logger.debug(f"XML scaling: {original_width}x{original_height} → {target_width}x{target_height}, "
                            f"scale factor: {scale_factor:.3f}, offset: ({offset_x:.1f}, {offset_y:.1f})")
            
            return temp_path
            
        except Exception as e:
            raise SVGProcessingError(f"Failed to scale SVG via XML: {e}")
    
    def _ensure_svg_namespace(self, root: ET.Element) -> None:
        """Ensure proper SVG namespace is set."""
        if 'xmlns' not in root.attrib and not root.tag.startswith('{'):
            root.set('xmlns', self.SVG_NAMESPACE)
    
    def _get_or_create_content_group(self, root: ET.Element) -> ET.Element:
        """Get existing content group or create a new one."""
        # Look for existing groups
        for child in root:
            if child.tag and self._get_tag_name(child.tag).lower() == 'g':
                return child
        
        # Create new group if none exists
        return self._create_content_group(root)
    
    def _get_tag_name(self, tag: str) -> str:
        """Extract tag name from potentially namespaced tag."""
        return tag.split('}')[-1] if '}' in tag else tag
    
    def _create_content_group(self, root: ET.Element) -> ET.Element:
        """Create a new content group and move existing content into it."""
        # Create namespace-aware group element
        group_tag = self._get_namespaced_tag(root, 'g')
        content_group = ET.Element(group_tag)
        
        # Move all existing content children to the new group (skip metadata elements)
        children_to_move = self._get_content_children(root)
        
        for child in children_to_move:
            root.remove(child)
            content_group.append(child)
        
        # Add the group to the root
        root.append(content_group)
        return content_group
    
    def _get_namespaced_tag(self, root: ET.Element, tag_name: str) -> str:
        """Get properly namespaced tag name."""
        if root.tag.startswith('{'):
            namespace = root.tag[1:root.tag.find('}')]
            return f'{{{namespace}}}{tag_name}'
        return tag_name
    
    def _get_content_children(self, root: ET.Element) -> List[ET.Element]:
        """Get all content children, excluding metadata elements."""
        children_to_move = []
        for child in root:
            if child.tag:
                tag_name = self._get_tag_name(child.tag).lower()
                # Only move actual content elements, skip metadata
                if tag_name not in self.METADATA_TAGS:
                    children_to_move.append(child)
        return children_to_move
    
    def _apply_scaling_transform(self, content_group: ET.Element, 
                                offset_x: float, offset_y: float, scale_factor: float) -> None:
        """Apply scaling transform to content group."""
        transform = f'translate({offset_x:.6f},{offset_y:.6f}) scale({scale_factor:.6f})'
        existing_transform = content_group.get('transform', '')
        
        if existing_transform:
            # Prepend our transform to existing ones
            content_group.set('transform', f'{transform} {existing_transform}')
        else:
            content_group.set('transform', transform)
    
    def _create_temp_svg_file(self) -> str:
        """Create a temporary SVG file and return its path."""
        temp_fd, temp_path = tempfile.mkstemp(suffix='.svg', prefix='xml_scaled_')
        os.close(temp_fd)
        return temp_path
    
    def _write_svg_file(self, tree: ET.ElementTree, temp_path: str) -> None:
        """Write SVG tree to file with proper formatting."""
        # Register the namespace to avoid ns0: prefixes
        root = tree.getroot()
        xmlns_value = root.get('xmlns')
        if xmlns_value:
            try:
                ET.register_namespace('', xmlns_value)
            except Exception:
                # Ignore namespace registration errors
                pass
        
        # Write the modified SVG with proper formatting
        with open(temp_path, 'wb') as f:
            f.write(b'<?xml version="1.0" encoding="UTF-8"?>\n')
            tree.write(f, encoding='utf-8', xml_declaration=False)
    
    def _validate_svg_output(self, temp_path: str) -> None:
        """Validate the generated SVG file."""
        if not os.path.exists(temp_path) or os.path.getsize(temp_path) == 0:
            raise SVGProcessingError("Generated SVG file is empty or invalid")
        
        # Basic validation - try to re-parse the generated file
        try:
            test_tree = ET.parse(temp_path)
            test_root = test_tree.getroot()
            if not test_root.tag or 'svg' not in test_root.tag.lower():
                raise SVGProcessingError("Generated file does not contain valid SVG root element")
        except ET.ParseError as e:
            raise SVGProcessingError(f"Generated SVG file is not valid XML: {e}")

class BatchProcessor:
    """
    A class to handle batch processing of SVG files using msdfgen.
    
    This class manages the overall workflow of processing multiple SVG files,
    maintaining folder structure, and coordinating between SVG preprocessing and msdfgen.
    """
    
    # Class constants
    SUCCESS_EXIT_CODE = 0
    FAILURE_EXIT_CODE = 1
    
    def __init__(self, logger: Optional[logging.Logger] = None):
        """
        Initialize the batch processor.
        
        Args:
            logger: Optional logger instance for logging operations
        """
        self.logger = logger or logging.getLogger(__name__)
        self.svg_processor = SVGProcessor(logger)
        self._processed_count = 0
        self._failed_count = 0
        self._temp_files: List[str] = []
        self._kept_preprocessed_files: List[str] = []
    
    def process_svg_files(self, 
                         input_folder: Union[str, Path], 
                         output_folder: Union[str, Path], 
                         dimensions: Tuple[int, int] = (64, 64), 
                         mode: str = "msdf", 
                         merge_paths: bool = False, 
                         keep_preprocessed_files: bool = False, 
                         preprocessed_folder: Optional[Union[str, Path]] = None, 
                         scale_size: Optional[Tuple[int, int]] = None) -> Tuple[int, int]:
        """
        Process all SVG files in input_folder and subfolders using msdfgen.
        
        Maintains the same folder structure in the output directory and provides
        comprehensive error handling and logging.
        
        Args:
            input_folder: Path to the input folder containing SVG files
            output_folder: Path to the output folder where PNG files will be saved
            dimensions: Width and height for output images
            mode: msdfgen mode (sdf, psdf, msdf, mtsdf)
            merge_paths: Whether to merge multiple SVG paths into one
            keep_preprocessed_files: Whether to keep preprocessed SVG files for debugging
            preprocessed_folder: Custom folder for preprocessed files
            scale_size: Target size to scale SVGs to before processing
            
        Returns:
            Tuple of (successful_count, failed_count)
            
        Raises:
            FileNotFoundError: If input folder doesn't exist
            ValueError: If invalid parameters are provided
        """
        # Validate input parameters
        self._validate_input_parameters(input_folder, output_folder)
        
        input_path = Path(input_folder)
        output_path = Path(output_folder)
        
        # Create output directory if it doesn't exist
        output_path.mkdir(parents=True, exist_ok=True)
        
        # Find all SVG files recursively
        svg_files = self._find_svg_files(input_path)
        
        if not svg_files:
            self.logger.info(f"No SVG files found in '{input_folder}'")
            return 0, 0
        
        self.logger.info(f"Found {len(svg_files)} SVG files to process...")
        
        # Reset counters
        self._reset_processing_state()
        
        return self._process_files_batch(
            svg_files, input_path, output_path, dimensions, mode, 
            merge_paths, keep_preprocessed_files, preprocessed_folder, scale_size
        )
    
    def _validate_input_parameters(self, input_folder: Union[str, Path], 
                                  output_folder: Union[str, Path]) -> None:
        """Validate input parameters."""
        input_path = Path(input_folder)
        
        if not input_path.exists():
            raise FileNotFoundError(f"Input folder '{input_folder}' does not exist.")
        
        if not input_path.is_dir():
            raise ValueError(f"Input path '{input_folder}' is not a directory.")
    
    def _find_svg_files(self, input_path: Path) -> List[Path]:
        """Find all SVG files recursively in the input path."""
        return list(input_path.rglob("*.svg"))
    
    def _reset_processing_state(self) -> None:
        """Reset processing state counters and lists."""
        self._processed_count = 0
        self._failed_count = 0
        self._temp_files.clear()
        self._kept_preprocessed_files.clear()

    def _process_files_batch(self, 
                            svg_files: List[Path], 
                            input_path: Path, 
                            output_path: Path, 
                            dimensions: Tuple[int, int], 
                            mode: str, 
                            merge_paths: bool, 
                            keep_preprocessed_files: bool, 
                            preprocessed_folder: Optional[Path], 
                            scale_size: Optional[Tuple[int, int]]) -> Tuple[int, int]:
        """
        Process a batch of SVG files.
        
        Args:
            svg_files: List of SVG file paths to process
            input_path: Base input directory path
            output_path: Base output directory path
            dimensions: Output image dimensions
            mode: msdfgen processing mode
            merge_paths: Whether to merge paths
            keep_preprocessed_files: Whether to keep preprocessed files
            preprocessed_folder: Custom folder for preprocessed files
            scale_size: Target scaling size
            
        Returns:
            Tuple of (successful_count, failed_count)
        """
        processed = 0
        failed = 0
        temp_files = []
        kept_preprocessed_files = []
        
        for i, svg_file in enumerate(svg_files, 1):
            try:
                success = self._process_single_file(
                    svg_file, input_path, output_path, dimensions, mode,
                    merge_paths, keep_preprocessed_files, preprocessed_folder,
                    scale_size, temp_files, kept_preprocessed_files, i, len(svg_files)
                )
                
                if success:
                    processed += 1
                else:
                    failed += 1
                    
            except Exception as e:
                failed += 1
                relative_path = svg_file.relative_to(input_path)
                self.logger.error(f"Exception processing {relative_path}: {str(e)}")
        
        # Clean up temporary files
        self._cleanup_temp_files(temp_files, keep_preprocessed_files)
        
        # Log results
        self._log_processing_results(processed, failed, kept_preprocessed_files)
        
        return processed, failed
    
    def _process_single_file(self, 
                            svg_file: Path, 
                            input_path: Path, 
                            output_path: Path, 
                            dimensions: Tuple[int, int], 
                            mode: str, 
                            merge_paths: bool, 
                            keep_preprocessed_files: bool, 
                            preprocessed_folder: Optional[Path], 
                            scale_size: Optional[Tuple[int, int]], 
                            temp_files: List[str], 
                            kept_preprocessed_files: List[str],
                            current_index: int,
                            total_count: int) -> bool:
        """
        Process a single SVG file.
        
        Args:
            svg_file: Path to the SVG file to process
            input_path: Base input directory path
            output_path: Base output directory path
            dimensions: Output image dimensions
            mode: msdfgen processing mode
            merge_paths: Whether to merge paths
            keep_preprocessed_files: Whether to keep preprocessed files
            preprocessed_folder: Custom folder for preprocessed files
            scale_size: Target scaling size
            temp_files: List to track temporary files
            kept_preprocessed_files: List to track kept preprocessed files
            current_index: Current file index (1-based)
            total_count: Total number of files to process
            
        Returns:
            True if processing was successful, False otherwise
        """
        processed_svg_path = None
        
        try:
            # Calculate relative path from input folder
            relative_path = svg_file.relative_to(input_path)
            
            # Create corresponding output path with PNG extension
            output_file = output_path / relative_path.with_suffix('.png')
            
            # Create output subdirectories if they don't exist
            output_file.parent.mkdir(parents=True, exist_ok=True)
            
            self.logger.info(f"[{current_index}/{total_count}] Processing: {relative_path}")
            
            # Preprocess SVG to merge paths if requested
            processed_svg_path = self.svg_processor.preprocess_svg(svg_file, merge_paths, scale_size)
            
            # Handle preprocessed file management
            self._handle_preprocessed_file(
                processed_svg_path, svg_file, relative_path, output_path,
                keep_preprocessed_files, preprocessed_folder, scale_size,
                temp_files, kept_preprocessed_files
            )
            
            # Run msdfgen
            return self._run_msdfgen(processed_svg_path, output_file, dimensions, mode, relative_path)
            
        finally:
            # Clean up temporary file if it was created and we're not keeping it
            self._cleanup_single_temp_file(processed_svg_path, svg_file, keep_preprocessed_files)
    
    def _handle_preprocessed_file(self, 
                                 processed_svg_path: str, 
                                 original_svg_file: Path, 
                                 relative_path: Path, 
                                 output_path: Path, 
                                 keep_preprocessed_files: bool, 
                                 preprocessed_folder: Optional[Path], 
                                 scale_size: Optional[Tuple[int, int]], 
                                 temp_files: List[str], 
                                 kept_preprocessed_files: List[str]) -> None:
        """
        Handle the preprocessed SVG file (save or track for cleanup).
        
        Args:
            processed_svg_path: Path to the processed SVG file
            original_svg_file: Path to the original SVG file
            relative_path: Relative path from input to current file
            output_path: Base output directory path
            keep_preprocessed_files: Whether to keep preprocessed files
            preprocessed_folder: Custom folder for preprocessed files
            scale_size: Target scaling size (for logging)
            temp_files: List to track temporary files
            kept_preprocessed_files: List to track kept preprocessed files
        """
        if processed_svg_path != str(original_svg_file):
            if keep_preprocessed_files:
                # Determine where to save the preprocessed file
                if preprocessed_folder:
                    preprocessed_base_path = Path(preprocessed_folder)
                    preprocessed_output_file = preprocessed_base_path / relative_path.with_suffix('.preprocessed.svg')
                else:
                    preprocessed_output_file = output_path / relative_path.with_suffix('.preprocessed.svg')
                
                preprocessed_output_file.parent.mkdir(parents=True, exist_ok=True)
                
                # Copy the temp file to the permanent location
                shutil.copy2(processed_svg_path, preprocessed_output_file)
                kept_preprocessed_files.append(str(preprocessed_output_file))
                
                scale_info = f" and scaled to {scale_size[0]}x{scale_size[1]}" if scale_size else ""
                processing_type = "XML scaling" if "xml_scaled" in processed_svg_path else "Inkscape"
                self.logger.info(f"  Processed with {processing_type}{scale_info} (saved preprocessed file: {preprocessed_output_file})")
            else:
                temp_files.append(processed_svg_path)
                scale_info = f" and scaled to {scale_size[0]}x{scale_size[1]}" if scale_size else ""
                processing_type = "XML scaling" if "xml_scaled" in processed_svg_path else "Inkscape"
                self.logger.info(f"  Processed with {processing_type}{scale_info}")
    
    def _run_msdfgen(self, 
                    svg_path: str, 
                    output_file: Path, 
                    dimensions: Tuple[int, int], 
                    mode: str, 
                    relative_path: Path) -> bool:
        """
        Run msdfgen on the processed SVG file.
        
        Args:
            svg_path: Path to the SVG file to process
            output_file: Path to the output PNG file
            dimensions: Output image dimensions
            mode: msdfgen processing mode
            relative_path: Relative path for logging
            
        Returns:
            True if msdfgen was successful, False otherwise
        """
        cmd = [
            "msdfgen.exe",
            mode,
            "-svg", svg_path,
            "-dimensions", str(dimensions[0]), str(dimensions[1]),
            "-o", str(output_file)
        ]
        
        try:
            result = subprocess.run(cmd, capture_output=True, text=True, check=False)
            
            if result.returncode == 0:
                self.logger.info(f"  ✓ Success: {output_file}")
                return True
            else:
                self.logger.error(f"  ✗ Failed: {relative_path}")
                if result.stderr:
                    self.logger.error(f"    Error: {result.stderr.strip()}")
                return False
                
        except (subprocess.SubprocessError, OSError) as e:
            self.logger.error(f"  ✗ Failed to run msdfgen for {relative_path}: {str(e)}")
            return False
    
    def _cleanup_single_temp_file(self, 
                                 processed_svg_path: str, 
                                 original_svg_file: Path, 
                                 keep_preprocessed_files: bool) -> None:
        """
        Clean up a single temporary file if needed.
        
        Args:
            processed_svg_path: Path to the processed SVG file
            original_svg_file: Path to the original SVG file
            keep_preprocessed_files: Whether preprocessed files are being kept
        """
        if (not keep_preprocessed_files and 
            processed_svg_path and 
            processed_svg_path != str(original_svg_file)):
            SVGProcessor._cleanup_temp_file(processed_svg_path)
    
    def _cleanup_temp_files(self, temp_files: List[str], keep_preprocessed_files: bool) -> None:
        """
        Clean up all temporary files if not keeping them.
        
        Args:
            temp_files: List of temporary file paths to clean up
            keep_preprocessed_files: Whether preprocessed files are being kept
        """
        if not keep_preprocessed_files:
            for temp_file in temp_files:
                SVGProcessor._cleanup_temp_file(temp_file)
    
    def _log_processing_results(self, 
                               processed: int, 
                               failed: int, 
                               kept_preprocessed_files: List[str]) -> None:
        """
        Log the final processing results.
        
        Args:
            processed: Number of successfully processed files
            failed: Number of failed files
            kept_preprocessed_files: List of kept preprocessed file paths
        """
        self.logger.info("\nProcessing complete!")
        self.logger.info(f"Successfully processed: {processed} files")
        self.logger.info(f"Failed: {failed} files")
        
        if kept_preprocessed_files:
            self.logger.info(f"\nKept {len(kept_preprocessed_files)} preprocessed SVG files for debugging:")
            for preprocessed_file in kept_preprocessed_files:
                self.logger.info(f"  {preprocessed_file}")


class ProcessingConfig:
    """Configuration class for batch processing parameters."""
    
    def __init__(self, 
                 dimensions: Tuple[int, int] = (64, 64),
                 mode: str = "msdf",
                 merge_paths: bool = False,
                 keep_preprocessed_files: bool = False,
                 preprocessed_folder: Optional[Path] = None,
                 scale_size: Optional[Tuple[int, int]] = None):
        self.dimensions = dimensions
        self.mode = mode
        self.merge_paths = merge_paths
        self.keep_preprocessed_files = keep_preprocessed_files
        self.preprocessed_folder = preprocessed_folder
        self.scale_size = scale_size
    
    def validate(self) -> None:
        """Validate configuration parameters."""
        if self.dimensions[0] <= 0 or self.dimensions[1] <= 0:
            raise ValueError("Dimensions must be positive integers")
        
        if self.mode not in ["sdf", "psdf", "msdf", "mtsdf"]:
            raise ValueError(f"Invalid mode: {self.mode}")


def setup_logging(verbose: bool = False) -> logging.Logger:
    """
    Set up logging configuration.
    
    Args:
        verbose: Whether to enable verbose logging
        
    Returns:
        Configured logger instance
    """
    level = logging.DEBUG if verbose else logging.INFO
    logging.basicConfig(
        level=level,
        format='%(message)s',
        handlers=[logging.StreamHandler()]
    )
    return logging.getLogger(__name__)


# Legacy function for backward compatibility
def process_svg_files(input_folder: str, 
                     output_folder: str, 
                     dimensions: Tuple[int, int] = (64, 64), 
                     mode: str = "msdf", 
                     merge_paths: bool = False, 
                     keep_preprocessed_files: bool = False, 
                     preprocessed_folder: Optional[str] = None, 
                     scale_size: Optional[Tuple[int, int]] = None) -> None:
    """
    Legacy function for backward compatibility.
    
    Process all SVG files in input_folder and subfolders using msdfgen,
    maintaining the same folder structure in the output directory.
    """
    logger = setup_logging()
    processor = BatchProcessor(logger)
    
    try:
        processed, failed = processor.process_svg_files(
            input_folder, output_folder, dimensions, mode, merge_paths,
            keep_preprocessed_files, Path(preprocessed_folder) if preprocessed_folder else None, scale_size
        )
    except Exception as e:
        logger.error(f"Processing failed: {e}")
        raise

def main():
    """
    Main entry point for the application.
    
    Parses command line arguments and initiates the batch processing workflow.
    """
    parser = argparse.ArgumentParser(
        description="Process SVG files with msdfgen while maintaining folder structure",
        formatter_class=argparse.RawDescriptionHelpFormatter,
        epilog="""
Examples:
  %(prog)s input_folder output_folder
  %(prog)s input_folder output_folder --width 128 --height 128
  %(prog)s input_folder output_folder --mode psdf --scale-width 64 --scale-height 64
  %(prog)s input_folder output_folder --keep-preprocessed-files --verbose
        """
    )
    
    # Required arguments
    parser.add_argument("input_folder", help="Input folder containing SVG files")
    parser.add_argument("output_folder", help="Output folder for generated PNG files")
    
    # Optional arguments
    parser.add_argument("--width", type=int, default=64, help="Output image width (default: 64)")
    parser.add_argument("--height", type=int, default=64, help="Output image height (default: 64)")
    parser.add_argument("--mode", choices=["sdf", "psdf", "msdf", "mtsdf"], default="msdf", 
                        help="msdfgen mode: sdf (monochrome SDF), psdf (perpendicular SDF), "
                             "msdf (multi-channel SDF, default), mtsdf (combined multi-channel and true SDF)")
    parser.add_argument("--merge-paths", action="store_true", 
                        help="Merge multiple SVG paths into a single path (default: False)")
    parser.add_argument("--keep-preprocessed-files", action="store_true", 
                        help="Keep preprocessed SVG files in output directory for debugging")
    parser.add_argument("--preprocessed-folder", type=str, 
                        help="Custom folder for preprocessed SVG files (default: output folder)")
    parser.add_argument("--scale-width", type=int, 
                        help="Scale SVG to this width before processing (requires --scale-height)")
    parser.add_argument("--scale-height", type=int, 
                        help="Scale SVG to this height before processing (requires --scale-width)")
    parser.add_argument("--verbose", "-v", action="store_true", 
                        help="Enable verbose logging")
    
    args = parser.parse_args()
    
    # Set up logging
    logger = setup_logging(args.verbose)
    
    try:
        # Validate scaling arguments
        scale_size = None
        if args.scale_width or args.scale_height:
            if not (args.scale_width and args.scale_height):
                parser.error("Both --scale-width and --scale-height must be provided together")
            scale_size = (args.scale_width, args.scale_height)
        
        # Validate dimensions
        if args.width <= 0 or args.height <= 0:
            parser.error("Width and height must be positive integers")
        
        # Get merge_paths boolean directly from args
        merge_paths = args.merge_paths
        
        # Create batch processor and run
        processor = BatchProcessor(logger)
        processed, failed = processor.process_svg_files(
            args.input_folder, 
            args.output_folder, 
            (args.width, args.height), 
            args.mode, 
            merge_paths, 
            args.keep_preprocessed_files, 
            Path(args.preprocessed_folder) if args.preprocessed_folder else None, 
            scale_size
        )
        
        # Exit with appropriate code
        if failed > 0:
            exit(1)
        else:
            exit(0)
            
    except KeyboardInterrupt:
        logger.info("\nProcessing interrupted by user")
        exit(130)
    except Exception as e:
        logger.error(f"Fatal error: {e}")
        if args.verbose:
            import traceback
            traceback.print_exc()
        exit(1)


def print_usage_examples():
    """Print usage examples and help information."""
    print("Usage examples:")
    print('  python gen.py "C:/path/to/svg/folder" "C:/path/to/output/folder"')
    print('  python gen.py "C:/path/to/svg/folder" "C:/path/to/output/folder" --width 128 --height 128')
    print('  python gen.py "C:/path/to/svg/folder" "C:/path/to/output/folder" --mode psdf')
    print('  python gen.py "C:/path/to/svg/folder" "C:/path/to/output/folder" --mode mtsdf --width 256 --height 256')
    print('  python gen.py "C:/path/to/svg/folder" "C:/path/to/output/folder" --merge-paths')
    print('  python gen.py "C:/path/to/svg/folder" "C:/path/to/output/folder" --keep-preprocessed-files')
    print('  python gen.py "C:/path/to/svg/folder" "C:/path/to/output/folder" --mode psdf --keep-preprocessed-files')
    print('  python gen.py "C:/path/to/svg/folder" "C:/path/to/output/folder" --keep-preprocessed-files --preprocessed-folder "C:/debug/preprocessed"')
    print('  python gen.py "C:/path/to/svg/folder" "C:/path/to/output/folder" --scale-width 64 --scale-height 64')
    print('  python gen.py "C:/path/to/svg/folder" "C:/path/to/output/folder" --mode psdf --scale-width 128 --scale-height 128')
    print("\nModes:")
    print("  sdf   - generates a conventional monochrome (true) signed distance field")
    print("  psdf  - generates a monochrome signed perpendicular distance field")
    print("  msdf  - generates a multi-channel signed distance field (default)")
    print("  mtsdf - generates a combined multi-channel and true signed distance field")
    print("\nPath Processing:")
    print("  By default, SVG paths are processed individually without merging")
    print("  Strokes are automatically converted to paths using Inkscape")
    print("  Use --merge-paths to merge multiple SVG paths into a single path")
    print("  Use --keep-preprocessed-files to save preprocessed SVG files for debugging")
    print("  Use --preprocessed-folder to specify a custom location for preprocessed files")
    print("  Use --scale-width and --scale-height to scale SVGs before processing")
    print("\nOr use command line arguments:")
    print("  python gen.py input_folder output_folder [--width WIDTH] [--height HEIGHT] [--mode MODE] [--merge-paths] [--keep-preprocessed-files] [--preprocessed-folder FOLDER] [--scale-width W --scale-height H]")

if __name__ == "__main__":
    # Example usage if run without command line arguments
    if len(os.sys.argv) == 1:
        print_usage_examples()
    else:
        main()